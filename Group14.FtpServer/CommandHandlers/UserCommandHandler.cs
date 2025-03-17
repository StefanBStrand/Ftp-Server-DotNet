namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the user command to set username
    /// </summary>
    internal class UserCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "USER";

        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            var parts = command.Split(' ', 2);
            if (parts.Length < 2)
                return Task.FromResult("501 Syntax error in parameters.");

            var username = parts[1].Trim();
            session.Username = username;
            session.IsAuthenticated = false;
            return Task.FromResult("331 Password required");
        }
    }
}
