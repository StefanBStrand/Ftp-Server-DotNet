using Group4.FtpServer.Handlers;
using Microsoft.Extensions.Logging;

namespace Group4.FtpServer
{
    /// <summary>
    /// Represents an FTP server that manages client connections and processes FTP commands.
    /// Dependencies are injected via the constructor to enable flexibility and testability.
    /// </summary>
    public class FtpServer : IFtpServer
    {
        private readonly IFtpConnectionListener _listener;
        private readonly IAsyncFtpCommandProcessor _commandProcessor;
        private bool _isRunning;
        private readonly ILogger<IFtpServer> _logger;
        private readonly IFtpSessionFactory _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the FtpServer class with the specified components and an optional logger.
        /// </summary>
        /// <param name="listener">The connection listener responsible for accepting incoming FTP client connections.</param>
        /// <param name="commandProcessor">The processor responsible for handling FTP commands from clients.</param>
        /// <param name="logger">The logger used to record server events. Can be null if logging is not required.</param>
        /// <param name="sessionFactory">The factory responsible for creating client session instances.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when listener, commandProcessor, sessionFactory is null.
        /// </exception>
        public FtpServer(
            IFtpConnectionListener listener,
            IAsyncFtpCommandProcessor commandProcessor,
            ILogger<IFtpServer> logger,
            IFtpSessionFactory sessionFactory)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener), "The connection listener can't be null.");

            if (commandProcessor == null)
                throw new ArgumentNullException(nameof(commandProcessor), "The command processor can't be null.");

            if (sessionFactory == null)
                throw new ArgumentNullException(nameof(sessionFactory), "The session factory can't be null.");

            _listener = listener;
            _commandProcessor = commandProcessor;
            _isRunning = false;
            _logger = logger;
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the FtpServer class with the specified components, excluding logging support.
        /// </summary>
        /// <param name="listener">The connection listener responsible for accepting incoming FTP client connections.</param>
        /// <param name="commandProcessor">The processor responsible for handling FTP commands from clients.</param>
        /// <param name="sessionFactory">The factory responsible for creating client session instances.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when listener, commandProcessor, sessionFactory is null.
        /// </exception>
        public FtpServer(
            IFtpConnectionListener listener,
            IAsyncFtpCommandProcessor commandProcessor,
            IFtpSessionFactory sessionFactory)
            : this(listener, commandProcessor, null, sessionFactory)
        { }


        /// <summary>
        /// Initializes a new instance of the FtpServer class with default component implementations.
        /// </summary>
        /// <param name="options">The configuration options for the server. If null, default options are used.</param>
        public FtpServer(FtpServerOptions options = null)
            : this(
                new TcpConnectionListener(options ??= new FtpServerOptions()),
                new FtpCommandProcessor(
                    new LocalFileStorage(options.RootPath),
                    new SimpleAuthenticationProvider(),
                    new UnixListFormatter(),
                    options),
                new DefaultFtpSessionFactory(new LocalFileStorage(options.RootPath)))
        { }

        /// <summary>
        /// Starts the FTP server asynchronously and begins accepting client connections.
        /// </summary>
        /// <returns>A task that resolves to a message indicating the result of the operation.</returns>
        public async Task<string> StartAsync()
        {
            if (_isRunning)
                return "Server is already running.";
            _isRunning = true;
            _listener.Start();
            _logger?.LogInformation("FTP server has started.");
            _ = Task.Run(() => AcceptConnectionsAsync());
            return "Server started.";
        }

        /// <summary>
        /// Stops the FTP server asynchronously, ceasing to accept new connections.
        /// </summary>
        /// <returns>A task that resolves to a message indicating the result of the operation.</returns>
        public Task<string> StopAsync()
        {
            if (!_isRunning)
                return Task.FromResult("Server is not running.");
            _isRunning = false;
            _listener.Stop();
            _logger?.LogInformation("FTP server has now stopped.");
            return Task.FromResult("Server stopped.");
        }

        /// <summary>
        /// Continuously accepts incoming client connections while the server is running.
        /// </summary>
        private async Task AcceptConnectionsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var connection = _listener.AcceptConnection();
                    _ = Task.Run(() => HandleClientAsync(connection));
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "Error accepting connection.");
                }
            }
        }

        private async Task HandleClientAsync(IAsyncFtpConnection connection)
        {
            var session = _sessionFactory.CreateSession();
            if (session == null)
                throw new InvalidOperationException("Session factory returned a null session.");

            using (connection)
            {
                await connection.SendResponseAsync("220 Welcome to Group-4 FTP server.");
                while (_isRunning)
                {
                    try
                    {
                        string command = await connection.ReadCommandAsync();
                        if (string.IsNullOrEmpty(command))
                            break;

                        var commandName = command.Split(' ')[0].ToUpper();

                        if (!session.IsAuthenticated &&
                            commandName != "USER" && commandName != "PASS" &&
                            commandName != "QUIT" && commandName != "AUTH")
                        {
                            await connection.SendResponseAsync("530 Please login with USER and PASS.");
                            continue;
                        }

                        string response = await _commandProcessor.ProcessCommandAsync(command, connection, session);

                        if (!string.IsNullOrEmpty(response))
                        {
                            await connection.SendResponseAsync(response);
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger?.LogError("Error handling the client's command: {Message}", e.Message);
                        await connection.SendResponseAsync($"500 Internal server error. {e.Message}");
                        break;
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError("Unexpected error: {Message}", e.Message);
                        await connection.SendResponseAsync($"500 Internal server error: {e.Message}");
                        break;
                    }
                }
            }
        }
    }
}
