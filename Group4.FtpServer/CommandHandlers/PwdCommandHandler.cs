namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the PWD command to display the current working directory.
    /// </summary>
    public class PwdCommandHandler : IAsyncFtpCommandHandler
    {
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SuccessResponseFormat = "257 \"{0}\" is the current directory";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "PWD";


        /// <summary>
        /// Processes the PWD command to return the current working directory.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string with the current directory path.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
            {
                return Task.FromResult(NotAuthenticatedResponse);
            }

            return Task.FromResult(string.Format(SuccessResponseFormat, session.CurrentDirectory));
        }
    }
}
