using Group14.FtpServer.Handlers;
using Microsoft.Extensions.Logging;

namespace Group14.FtpServer
{
    /// <summary>
    /// Standard implementation managing client connections and commands.
    /// Dependencies are injected through the constructor.
    /// </summary>
    public class FtpServer : IFtpServer
    {
        private readonly IFtpConnectionListener _listener;
        private readonly IFtpCommandProcessor _commandProcessor;
        private bool _isRunning;
        private readonly ILogger _logger;
        private readonly IFtpSessionFactory _sessionFactory;

        /// <summary>
        /// Initializes a new FTP server with the specified components and a logger.
        /// </summary>
        /// <param name="listener">The listener to use.</param>
        /// <param name="commandProcessor">The command processor to use.</param>
        /// <param name="sessionFactory">The factory for creating client sessions.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either listener, command processor or or session factory is null.
        /// </exception>
        public FtpServer (
            IFtpConnectionListener listener,
            IFtpCommandProcessor commandProcessor, 
            ILogger logger,
            IFtpSessionFactory sessionFactory )
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
        /// Initializes a new FTP server with the specified components, without logging.
        /// </summary>
        /// <param name="listener">The listener to use.</param>
        /// <param name="commandProcessor">The command processor to use.</param>
        /// <param name="sessionFactory">The factory for creating client sessions.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either listener, command processor, authentication provider or session factory is null.
        /// </exception>
        public FtpServer(
            IFtpConnectionListener listener,
            IFtpCommandProcessor commandProcessor,
            IFtpSessionFactory sessionFactory)
            : this(listener, commandProcessor, null, sessionFactory)
        { }


        /// <summary>
        /// Initializes a new FTP server with default settings.
        /// </summary>
        /// <param name="options">The configuration options for the server. If null, default options are used.</param>
        public FtpServer(FtpServerOptions options = null)
            : this(
                new TcpConnectionListener(options ??= new FtpServerOptions()),
                new DefaultFtpCommandProcessor(
                    new LocalFileStorage(options.RootPath),
                    new SimpleAuthenticationProvider(),
                    new UnixListFormatter(),
                    options),
                new DefaultFtpSessionFactory(new LocalFileStorage(options.RootPath)))
        { }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            if (_isRunning) 
                return;

            _isRunning = true;
            _listener.Start();
            _logger?.LogInformation("FTP server has started.");
            new Thread(AcceptConnections).Start();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _listener.Stop();
            _logger?.LogInformation("FTP server has now stopped.");
        }

        /// <summary>
        /// Accepts incoming connections.
        /// </summary>
        private void AcceptConnections()
        {
            while (_isRunning)
            {
                try
                {
                    var connection = _listener.AcceptConnection();
                    new Thread(() => HandleClient(connection)).Start();
                }
                catch (Exception e) // TO DO -> betetr exception here
                {
                    _logger?.LogError(e, "Error accepting connection.");
                }
            }
        }

        private void HandleClient(IFtpConnection connection)
        {
            var session = _sessionFactory.CreateSession();
            if (session == null)
                throw new InvalidOperationException("Session factory returned a null session.");

            using (connection)
            {
                connection.SendResponse("220 Welcome to Group-4 FTP server.");
                while (_isRunning)
                {
                    try
                    {
                        var command = connection.ReadCommand();

                        if (string.IsNullOrEmpty(command))
                            break; // client disconnect

                        var commandName = command.Split(' ')[0].ToUpper();
                        if (!session.IsAuthenticated && commandName != "USER" && commandName != "PASS" && commandName != "QUIT")
                        {
                            connection.SendResponse("530 Please login with USER and PASS.");
                            continue;
                        }

                        var response = _commandProcessor.ProcessCommand(command, connection, session);
                        connection.SendResponse(response);
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger?.LogError("Error handling the clients command.");
                        connection.SendResponse($"500 Internal server error. {e.Message}");
                        break;
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError("Something unexpected happened. Internal server error.");
                        connection.SendResponse($"500 Internal server error: {e.Message}");
                        break;
                    }
                }
            }
        }
    }
}
