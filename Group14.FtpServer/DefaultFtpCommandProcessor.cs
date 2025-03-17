using Group14.FtpServer.Factories;
using Microsoft.Extensions.Logging;

namespace Group14.FtpServer
{
    /// <summary>
    /// Standard implementation to reroute the comands
    /// </summary>
    public class DefaultFtpCommandProcessor : IAsyncFtpCommandProcessor
    {
        private readonly IFtpCommandHandlerFactory _commandHandlerFactory;
        private readonly ILogger<IAsyncFtpCommandProcessor> _logger;

        public DefaultFtpCommandProcessor(
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

        public DefaultFtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            ILogger<IAsyncFtpCommandProcessor> logger)
        : this(backendStorage, authenticationProvider, listFormatter, null, logger) { }

        public DefaultFtpCommandProcessor(
            IBackendStorage backendStorage,
            IAuthenticationProvider authenticationProvider,
            IListFormatter listFormatter,
            FtpServerOptions options)
        : this(backendStorage, authenticationProvider, listFormatter, options, null) { }

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
