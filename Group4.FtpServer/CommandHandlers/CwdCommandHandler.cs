namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the CWD command to change the current working directory.
    /// </summary>
    public class CwdCommandHandler : IAsyncFtpCommandHandler
    {
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SyntaxErrorResponse = "501 Syntax error in parameters.";
        private const string SuccessResponse = "250 Directory changed successfully.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "CWD";

        /// <summary>
        /// Processes the CWD command to change the current directory to the specified path.
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

            var commandArguments = command.Split(' ', 2);
            if (commandArguments.Length < 2)
            {
                return Task.FromResult(SyntaxErrorResponse);
            }

            var targetDirectory = commandArguments[1].Trim();
            var newPath = Path.Combine(session.CurrentDirectory, targetDirectory).Replace('\\', '/');
            session.CurrentDirectory = newPath;

            return Task.FromResult(SuccessResponse);
        }
    }
}
