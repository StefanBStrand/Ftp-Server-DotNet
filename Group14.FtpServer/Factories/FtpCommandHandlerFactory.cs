using System;
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
        private readonly FtpServerOptions _options;

        public FtpCommandHandlerFactory(IBackendStorage backendStorage,
                                        IAuthenticationProvider authenticationProvider,
                                        IListFormatter listFormatter,
                                        FtpServerOptions options)
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
            _options = options;
        }

        public IAsyncFtpCommandHandler CreateHandler(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name cannot be null or empty.", nameof(commandName));

            return commandName.ToUpperInvariant() switch
            {
                "AUTH" => new AuthTlsCommandHandler(_options),
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
                "SYST" => new SystCommandHandler(), 
                "FEAT" => new FeatCommandHandler(), 
                "PBSZ" => new PbszCommandHandler(), 
                _ => throw new NotSupportedException($"Command '{commandName}' is not supported by this server.")
            };
        }
    }
}
