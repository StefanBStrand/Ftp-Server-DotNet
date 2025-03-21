namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the type command to set transfer mode
    /// </summary>
    public class TypeCommandHandler : IAsyncFtpCommandHandler
    {
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SyntaxErrorResponse = "501 Syntax error in parameters.";
        private const string InvalidTypeResponse = "504 Command not implemented for that parameter.";
        private const string SuccessResponseFormat = "200 Type set to {0}.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "TYPE";

        /// <summary>
        /// Processes the TYPE command to set the file transfer type.
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

            var transferType = commandArguments[1].Trim().ToUpper();
            if (transferType == "A" || transferType == "I")
            {
                return Task.FromResult(string.Format(SuccessResponseFormat, transferType));
            }

            return Task.FromResult(InvalidTypeResponse);
        }
    }
}
