namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the PORT command to set up an active mode data connection (not yet implemented).
    /// </summary>
    public class PortCommandHandler : IAsyncFtpCommandHandler
    {
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string NotImplementedResponse = "502 Active mode not implemented yet.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "PORT";

        /// <summary>
        /// Processes the PORT command to set up an active mode data connection.
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

            return Task.FromResult(NotImplementedResponse);
        }
    }
}
