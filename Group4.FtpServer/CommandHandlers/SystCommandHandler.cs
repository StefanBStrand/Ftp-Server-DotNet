namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the SYST command to identify the server's system type.
    /// </summary>
    public class SystCommandHandler : IAsyncFtpCommandHandler
    {
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SystemTypeResponse = "215 UNIX Type: L8";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "SYST";

        /// <summary>
        /// Processes the SYST command to return the server's system type.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the system type.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
            {
                return Task.FromResult(NotAuthenticatedResponse);
            }

            return Task.FromResult(SystemTypeResponse);
        }
    }
}
