namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the CDUP command to move up one directory level.
    /// </summary>
    internal class CdupCommandHandler : IFtpCommandHandler
    {
        /// <summary>
        /// Processes the CDUP command and changes directory up one level.
        /// </summary>
        /// <param name="command">The full command string from the client.</param>
        /// <param name="connection">The connection to the client.</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message.</returns>
        public string HandleCommand(string command, IFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return "530 Please login with USER and PASS.";

            if (session.CurrentDirectory == "/") // root check so we dont go below
                return "550 Already at root directory.";

            var currentDir = Path.GetDirectoryName(session.CurrentDirectory);
            if (currentDir == null)
            {
                session.CurrentDirectory = "/";
            }
            else
            {
                session.CurrentDirectory = currentDir.Replace('\\', '/');
            }

            return "250 Directory changed successfully.";
        }
    }
}
