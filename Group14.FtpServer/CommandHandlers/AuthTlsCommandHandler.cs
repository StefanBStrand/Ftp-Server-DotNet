namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the AUTH TLS command to upgrade a plaintext connection to a TLS-secured connection.
    /// </summary>
    public class AuthTlsCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly FtpServerOptions _options;

        public AuthTlsCommandHandler(FtpServerOptions options)
        {
            _options = options;
        }

        public string Command => "AUTH";

        public async Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!_options.EnableTls)
            {
                return "502 Command not implemented.";
            }

            await connection.SendResponseAsync("234 Proceed with negotiation.");
            if (connection is AsyncTcpFtpConnection tcpConnection)
            {
                await tcpConnection.UpgradeToTlsAsync();
                return null;
            }
            else
            {
                throw new InvalidOperationException("Connection does not support TLS upgrade.");
            }
        }
    }
}