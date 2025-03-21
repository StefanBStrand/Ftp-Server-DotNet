namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the FEAT command to list supported FTP features.
    /// </summary>
    public class FeatCommandHandler : IAsyncFtpCommandHandler
    {
        private const string FeaturesResponse =
            "211-Features:\r\n" +
            " AUTH TLS\r\n" +
            " PBSZ\r\n" +
            " PROT\r\n" +
            " UTF8\r\n" +
            "211 End";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "FEAT";

        /// <summary>
        /// Processes the FEAT command to return a list of supported FTP features.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string listing the supported features.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            return Task.FromResult(FeaturesResponse);
        }
    }
}
