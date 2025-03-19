namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the PWD command to show current directory
    /// </summary>
    public class PwdCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "PWD";

        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return Task.FromResult("530 Please login with USER and PASS.");
            return Task.FromResult($"257 \"{session.CurrentDirectory}\" is the current directory");
        }
    }
}
