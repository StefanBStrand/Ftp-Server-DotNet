namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the CDUP command to move up one directory level.
    /// </summary>
    public class CdupCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "CDUP";

        /// <summary>
        /// Processes the CDUP command and changes directory up one level.
        /// </summary>
        /// <param name="command">The full command string from the client.</param>
        /// <param name="connection">The connection to the client.</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return Task.FromResult("530 Please login with USER and PASS.");

            if (session.CurrentDirectory == "/")
                return Task.FromResult("550 Already at root directory.");

            var currentDir = Path.GetDirectoryName(session.CurrentDirectory);
            session.CurrentDirectory = currentDir == null ? "/" : currentDir.Replace('\\', '/');
            return Task.FromResult("250 Directory changed successfully.");
        }
    }
}
