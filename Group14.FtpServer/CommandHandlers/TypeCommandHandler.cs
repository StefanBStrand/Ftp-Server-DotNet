namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the type command to set transfer mode
    /// </summary>
    internal class TypeCommandHandler : IFtpCommandHandler
    {
        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public string HandleCommand(string command, IFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return "530 Please login with USER and PASS.";

            var parts = command.Split(' ', 2);
            if (parts.Length < 2)
                return "501 Syntax error in parameters.";

            var type = parts[1].Trim().ToUpper();
            if (type == "A" || type == "I") // ASCII or Binary
                return $"200 Type set to {type}.";

            return "504 Command not implemented for that parameter.";
        }
    }
}
