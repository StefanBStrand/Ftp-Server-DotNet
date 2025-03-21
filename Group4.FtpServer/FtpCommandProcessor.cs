using Group4.FtpServer.Factories;
using Microsoft.Extensions.Logging;

namespace Group4.FtpServer
{
    /// <summary>
    /// Processes FTP commands by routing them to the appropriate handlers.
    /// </summary>
    public class FtpCommandProcessor : IAsyncFtpCommandProcessor
    {
        private readonly IFtpCommandHandlerFactory _commandHandlerFactory;
        private readonly ILogger<IAsyncFtpCommandProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the FtpCommandProcessor> class with the specified dependencies.
        /// </summary>
        /// <param name="backendStorage">The storage backend for file operations.</param>
        /// <param name="authenticationProvider">The authentication provider for user validation.</param>
        /// <param name="listFormatter">The formatter for directory listings.</param>
        /// <param name="options">The server configuration options. Can be null.</param>
        /// <param name="logger">The logger for command processing events. Can be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions options,
            ILogger<IAsyncFtpCommandProcessor> logger)
        {
            if (backendStorage == null)
                throw new ArgumentNullException(nameof(backendStorage), "Backend storage can't be null.");

            if (authenticationProvider == null)
                throw new ArgumentNullException(nameof(authenticationProvider), "Authentication provider can't be null.");

            if (listFormatter == null)
                throw new ArgumentNullException(nameof(listFormatter), "List formatter can't be null.");

            if (options == null)
                throw new ArgumentNullException(nameof(options), "The options provided can't be null");

            _logger = logger;
            _commandHandlerFactory = new FtpCommandHandlerFactory(backendStorage, authenticationProvider, listFormatter, options);
        }

        /// <summary>
        /// Initializes a new instance of theFtpCommandProcessor class without server options.
        /// </summary>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            ILogger<IAsyncFtpCommandProcessor> logger)
        : this(backendStorage, authenticationProvider, listFormatter, null, logger) { }


        /// <summary>
        /// Initializes a new instance of the FtpCommandProcessor class without a logger.
        /// </summary>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions options)
        : this(backendStorage, authenticationProvider, listFormatter, options, null) { }


        /// <summary>
        /// Processes an FTP command asynchronously and returns the response.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The connection to the client.</param>
        /// <param name="session">The current session state.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the FTP response code and message.</returns>
        /// <exception cref="ArgumentException">Thrown if the command is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if no handler is found for the command.</exception>
        public async Task<string> ProcessCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            _logger?.LogInformation("Received command: {Command}", command);

            if (string.IsNullOrEmpty(command))
                return "500 Invalid command.";

            var commandName = command.Split(' ')[0];
            var handler = _commandHandlerFactory.CreateHandler(commandName);
            return await handler.HandleCommandAsync(command, connection, session);
        }
    }
}
