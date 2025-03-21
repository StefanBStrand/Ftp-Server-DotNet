namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the user command to set username
    /// </summary>
    public class UserCommandHandler : IAsyncFtpCommandHandler
    {
        private const string SyntaxErrorResponse = "501 Syntax error in parameters.";
        private const string PasswordRequiredResponse = "331 Password required";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "USER";

        /// <summary>
        /// Processes the USER command to set the username and prepare for password authentication.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the next step in authentication.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            var commandArguments = command.Split(' ', 2);
            if (commandArguments.Length < 2)
            {
                return Task.FromResult(SyntaxErrorResponse);
            }

            var username = commandArguments[1].Trim();
            session.Username = username;
            session.IsAuthenticated = false;

            return Task.FromResult(PasswordRequiredResponse);
        }
    }
}
