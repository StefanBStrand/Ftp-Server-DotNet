using Group14.FtpServer.Factories;

namespace Group14.FtpServer
{
    /// <summary>
    /// Standard implementation to reroute the comands
    /// </summary>
    internal class DefaultFtpCommandProcessor : IFtpCommandProcessor
    {
        private readonly IFtpCommandHandlerFactory _commandHandlerFactory;
        private readonly IListFormatter _formatter;

        public DefaultFtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions options)
        {
            if (backendStorage == null)
                throw new ArgumentNullException(nameof(backendStorage), "Backend storage can't be null.");

            if (authenticationProvider == null)
                throw new ArgumentNullException(nameof(authenticationProvider), "Authentication provider can't be null.");

            if (listFormatter == null)
                throw new ArgumentNullException(nameof(listFormatter), "List formatter can't be null.");

            if (options == null)
                throw new ArgumentNullException(nameof(options), "The options provided can't be null");

            _commandHandlerFactory = new FtpCommandHandlerFactory(backendStorage, authenticationProvider, listFormatter, options);
        }

        public DefaultFtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter)
        : this(backendStorage, authenticationProvider, listFormatter, null) { }

        public string ProcessCommand(string command, IFtpConnection connection, IFtpSession session)
        {
            if (string.IsNullOrEmpty(command))
                return "500 Invalid command.";

            var commandName = command.Split(' ')[0];
            var handler = _commandHandlerFactory.CreateHandler(commandName);
            return handler.HandleCommand(command, connection, session);
        }
    }
}
