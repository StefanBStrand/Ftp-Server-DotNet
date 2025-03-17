namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the port command for active mode.
    /// </summary>
    internal class PortCommandHandler : IFtpCommandHandler
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

            return "502 Active mode not implemented yet."; 
        }
    }
}
