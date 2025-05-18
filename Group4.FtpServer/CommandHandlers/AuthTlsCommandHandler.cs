namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the AUTH TLS command to upgrade a plaintext connection to a TLS-secured connection.
    /// </summary>
    public class AuthTlsCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly FtpServerOptions _serverOptions;
        private const string NotImplementedResponse = "502 Command not implemented.";
        private const string NegotiationResponse = "234 Proceed with negotiation.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "AUTH";

        /// <summary>
        /// Initializes a new instance of the AuthTlsCommandHandler class.
        /// </summary>
        /// <param name="serverOptions">The server options containing TLS configuration settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if server options is null.</exception>
        public AuthTlsCommandHandler(FtpServerOptions serverOptions)
        {
            if (serverOptions == null) 
                throw new ArgumentNullException(nameof(serverOptions), "Server options cannot be null.");

            _serverOptions = serverOptions;
        }

        /// <summary>
        /// Processes the AUTH TLS command to upgrade to TLS-secured connection.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the result, or null if the connection is upgraded successfully.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the connection does not support TLS upgrades.</exception>
        public async Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!_serverOptions.EnableTls)
            {
                return NotImplementedResponse;
            }


            await connection.SendResponseAsync(NegotiationResponse);
            if (connection is TcpFtpConnection tcpConnection)
            {
                await tcpConnection.UpgradeToTlsAsync();
                return null!;
            }

            throw new InvalidOperationException("Connection does not support TLS upgrade.");
        }
    }
}