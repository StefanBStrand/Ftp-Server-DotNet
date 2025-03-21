using Group4.FtpServer.CommandHandlers;

namespace Group4.FtpServer.Factories
{
    /// <summary>
    /// Factory for creating FTP command handlers based on the command name.
    /// </summary>
    internal class FtpCommandHandlerFactory : IFtpCommandHandlerFactory
    {
        private readonly IBackendStorage _storageBackend;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly PasvCommandHandler _passiveModeHandler;
        private readonly IListFormatter _listFormatter;
        private readonly FtpServerOptions _serverOptions;

        /// <summary>
        /// Initializes a new instance of the FtpCommandHandlerFactory class.
        /// </summary>
        /// <param name="storageBackend">The backend storage to use for file operations.</param>
        /// <param name="authenticationProvider">The authentication provider used to verify user credentials.</param>
        /// <param name="listFormatter">The formatter used for generating directory listings.</param>
        /// <param name="serverOptions">The server configuration options.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when storageBackend, authenticationProvider,
        /// listFormatter or serverOptions is null.
        /// </exception>
        public FtpCommandHandlerFactory(IBackendStorage storageBackend,
                                        IAuthenticationProvider authenticationProvider,
                                        IListFormatter listFormatter,
                                        FtpServerOptions serverOptions)
        {
            if (storageBackend == null)
                throw new ArgumentNullException(nameof(storageBackend), "The backend storage type can't be null.");
            if (authenticationProvider == null)
                throw new ArgumentNullException(nameof(authenticationProvider), "The authentication provider can't be null.");
            if (listFormatter == null)
                throw new ArgumentNullException(nameof(listFormatter), "The list formatter can't be null.");
            if (serverOptions == null)
                throw new ArgumentNullException(nameof(serverOptions), "The server options cant be null.");

            _storageBackend = storageBackend;
            _authenticationProvider = authenticationProvider;
            _passiveModeHandler = new PasvCommandHandler(serverOptions);
            _listFormatter = listFormatter;
            _serverOptions = serverOptions;
        }

        /// <summary>
        /// Creates a command handler for the specified FTP command.
        /// </summary>
        /// <param name="commandName">The name of the FTP command.</param>
        /// <returns>An instance of a command handler that can process the command.</returns>
        /// <exception cref="ArgumentException">Thrown if commandName is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if the command is not supported by the server.</exception>
        public IAsyncFtpCommandHandler CreateHandler(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw new ArgumentException("Command name can't be null or empty.", nameof(commandName));
            }

            return GetHandlerForCommand(commandName.ToUpperInvariant());
        }

        private IAsyncFtpCommandHandler GetHandlerForCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name can't be null or empty.", nameof(commandName));

            return commandName.ToUpperInvariant() switch
            {
                "AUTH" => new AuthTlsCommandHandler(_serverOptions),
                "CWD" => new CwdCommandHandler(),
                "CDUP" => new CdupCommandHandler(),
                "DELE" => new DeleCommandHandler(_storageBackend),
                "LIST" => new ListCommandHandler(_storageBackend, _passiveModeHandler, _listFormatter),
                "PASV" => _passiveModeHandler,
                "PORT" => new PortCommandHandler(),
                "PWD" => new PwdCommandHandler(),
                "RETR" => new RetrCommandHandler(_storageBackend, _passiveModeHandler),
                "STOR" => new StoreCommandHandler(_storageBackend, _passiveModeHandler),
                "TYPE" => new TypeCommandHandler(),
                "USER" => new UserCommandHandler(),
                "PASS" => new PassCommandHandler(_authenticationProvider),
                "QUIT" => new QuitCommandHandler(),
                "SYST" => new SystCommandHandler(), 
                "FEAT" => new FeatCommandHandler(), 
                "PBSZ" => new PbszCommandHandler(), 
                _ => throw new NotSupportedException($"Command '{commandName}' is not supported by this server.")
            };
        }
    }
}
