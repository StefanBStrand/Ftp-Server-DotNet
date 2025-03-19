namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the quit command to end the session
    /// </summary>
    public class QuitCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "QUIT";

        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            connection.SendResponseAsync("221 GOodbye");
            connection.CloseAsync();
            return Task.FromResult("221 GOodbye");
        }
    }
}