namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the quit command to end the session
    /// </summary>
    internal class QuitCommandHandler : IFtpCommandHandler
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
            connection.SendResponse("221 GOodbye");
            connection.Close(); // 
            return "221 Goodbye";
        }
    }
}