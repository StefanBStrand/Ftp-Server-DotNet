namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the PBSZ command to set the protection buffer size for secure data transfers.
    /// </summary>
    public class PbszCommandHandler : IAsyncFtpCommandHandler
    {
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SuccessResponse = "200 PBSZ command successful.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "PBSZ";

        /// <summary>
        /// Processes the PBSZ command to acknowledge the protection buffer size setting.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the result of the operation.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
            {
                return Task.FromResult(NotAuthenticatedResponse);
            }

            return Task.FromResult(SuccessResponse);
        }
    }
}
