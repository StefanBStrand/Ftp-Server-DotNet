namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the CDUP command to move the current directory up one level.
    /// </summary>>
    public class CdupCommandHandler : IAsyncFtpCommandHandler
    {

        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string RootDirectoryResponse = "550 Already at root directory.";
        private const string SuccessResponse = "250 Directory changed successfully.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "CDUP";


        /// <summary>
        /// Processes the CDUP command to change the current directory up one level.
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

            if (session.CurrentDirectory == "/")
            {
                return Task.FromResult(RootDirectoryResponse);
            }

            var parentDirectory = Path.GetDirectoryName(session.CurrentDirectory);
            session.CurrentDirectory = parentDirectory ?? "/".Replace('\\', '/');
            return Task.FromResult(SuccessResponse);
        }
    }
}
