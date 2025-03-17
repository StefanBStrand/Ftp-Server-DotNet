using Group14.FtpServer.CommandHandlers;
namespace Group14.FtpServer.Factories
{
    /// <summary>
    /// Provides a factory for creating FTP command handlers.
    /// </summary>
    internal class FtpCommandHandlerFactory : IFtpCommandHandlerFactory
    {
        private readonly IBackendStorage _backendStorage;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly PasvCommandHandler _pasvHandler;
        private readonly IListFormatter _listFormatter;

        /// <summary>
        /// Initializes a new instance of the FtpCommandHandlerFactory class.
        /// </summary>
        /// <param name="backendStorage">The storage backend for file operations.</param>
        /// <param name="authenticationProvider">The provider for user authentication.</param>
        /// <param name="listFormatter">The formatter for directory listings.</param>
        /// <param name="options">The server options, including PASV configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if backendStorage, authenticationProvider, list formatter or options is null.
        /// </exception>
        public FtpCommandHandlerFactory(IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions options
            )
        {
            if (backendStorage == null)
                throw new ArgumentNullException(nameof(backendStorage), "The backend storage type can't be null.");

            if (authenticationProvider == null)
                throw new ArgumentNullException(nameof(authenticationProvider), "The authentication provider can't be null.");

            if (listFormatter == null)
                throw new ArgumentNullException(nameof(listFormatter), "The list formatter can't be null.");

            if (options == null)
                throw new ArgumentNullException(nameof(options), "The options provided can't be null.");

            _backendStorage = backendStorage;
            _authenticationProvider = authenticationProvider; 
            _pasvHandler = new PasvCommandHandler(options);
            _listFormatter = listFormatter;
        }

        /// <summary>
        /// Creates a command handler for the specified FTP command.
        /// </summary>
        /// <param name="commandName">The name of the FTP command.</param>
        /// <returns>An instance of a command handler that can process the command.</returns>
        /// <exception cref="ArgumentException">Thrown if commandName is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if the command is not supported.</exception>
        public IFtpCommandHandler CreateHandler(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name cannot be null or empty.", nameof(commandName));

            return commandName.ToUpperInvariant() switch 
            {
                "CWD" => new CwdCommandHandler(),
                "CDUP" => new CdupCommandHandler(),
                "DELE" => new DeleCommandHandler(_backendStorage),
                "LIST" => new ListCommandHandler(_backendStorage, _pasvHandler, _listFormatter),
                "PASV" => _pasvHandler,
                "PORT" => new PortCommandHandler(),
                "PWD" => new PwdCommandHandler(),
                "RETR" => new RetrCommandHandler(_backendStorage, _pasvHandler),
                "STOR" => new StoreCommandHandler(_backendStorage, _pasvHandler),
                "TYPE" => new TypeCommandHandler(),
                "USER" => new UserCommandHandler(),
                "PASS" => new PassCommandHandler(_authenticationProvider),
                "QUIT" => new QuitCommandHandler(),
                _ => throw new NotSupportedException($"Command '{commandName}' is not supported by this server.")
            };
        }
    }
}