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
        private readonly ILogger<IAsyncFtpCommandProcessor>? _logger;
        private readonly ICommandAuthorizer? _commandAuthorizer;

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
            FtpServerOptions? options,
            ILogger<IAsyncFtpCommandProcessor>? logger,
            ICommandAuthorizer? commandAuthorizer)
        {
            if (backendStorage == null)
                throw new ArgumentNullException(nameof(backendStorage), "Backend storage can't be null.");

            if (authenticationProvider == null)
                throw new ArgumentNullException(nameof(authenticationProvider), "Authentication provider can't be null.");

            if (listFormatter == null)
                throw new ArgumentNullException(nameof(listFormatter), "List formatter can't be null.");

            _logger = logger;
            var actualOptions = options ?? new FtpServerOptions();
            _commandHandlerFactory = new FtpCommandHandlerFactory(backendStorage, authenticationProvider, listFormatter, actualOptions);
            _commandAuthorizer = commandAuthorizer;
        }

        /// <summary>
        /// Initializes a new instance of the FtpCommandProcessor class without an authorizer.
        /// </summary>
        /// <param name="backendStorage">The storage backend.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="listFormatter">The list formatter.</param>
        /// <param name="options">The FTP server options.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions? options,
            ILogger<IAsyncFtpCommandProcessor> logger)
            : this(backendStorage, authenticationProvider, listFormatter, options, logger, null)
        { }

        /// <summary>
        /// Initializes a new instance of the FtpCommandProcessor class without a logger.
        /// </summary>
        /// <param name="backendStorage">The storage backend.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="listFormatter">The list formatter.</param>
        /// <param name="options">The FTP server options.</param>
        /// <param name="authorizer">The command authorizer.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions? options,
            ICommandAuthorizer authorizer)
            : this(backendStorage, authenticationProvider, listFormatter, options, null, authorizer)
        { }

        /// <summary>
        /// Initializes a new instance of the FtpCommandProcessor class without a logger or authorizer.
        /// </summary>
        /// <param name="backendStorage">The storage backend.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="listFormatter">The list formatter.</param>
        /// <param name="options">The FTP server options.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions options)
            : this(backendStorage, authenticationProvider, listFormatter, options, null, null)
        { }

        /// <summary>
        /// Initializes a new instance of the FtpCommandProcessor class with only a logger (no options or authorizer).
        /// </summary>
        /// <param name="backendStorage">The storage backend.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="listFormatter">The list formatter.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            ILogger<IAsyncFtpCommandProcessor> logger)
            : this(backendStorage, authenticationProvider, listFormatter, null, logger, null)
        { }

        /// <summary>
        /// Initializes a new instance of the FtpCommandProcessor class with only required dependencies.
        /// </summary>
        /// <param name="backendStorage">The storage backend.</param>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="listFormatter">The list formatter.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
        public FtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter)
            : this(backendStorage, authenticationProvider, listFormatter, null, null, null)
        { }


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

            if (_commandAuthorizer != null && !_commandAuthorizer.isAuthorized(session, commandName)) {
                return "550 Permission denied.";
            }

            var handler = _commandHandlerFactory.CreateHandler(commandName);
            return await handler.HandleCommandAsync(command, connection, session);
        }
    }
}
