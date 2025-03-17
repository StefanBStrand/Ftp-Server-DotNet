namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Defines a contract for an FTP command handler.
    /// </summary>
    public interface IFtpCommandHandler
    {
        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public string HandleCommand(string command, IFtpConnection connection, IFtpSession session);
    }
}
