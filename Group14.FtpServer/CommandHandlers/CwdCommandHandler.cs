namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the CWD command to change the current directory.
    /// </summary>
    internal class CwdCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "CWD";
        /// <summary>
        /// Processes the cwd command and changes to the specified directory
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return Task.FromResult("530 Please login with USER and PASS.");

            var commandParts = command.Split(' ', 2);
            if (commandParts.Length < 2)
                return Task.FromResult("501 Syntax error in parameters.");

            var newDir = commandParts[1].Trim();
            string newPath = Path.Combine(session.CurrentDirectory, newDir);
            session.CurrentDirectory = newPath.Replace('\\', '/');

            return Task.FromResult("250 Directory changed successfully.");
        }
    }
}
