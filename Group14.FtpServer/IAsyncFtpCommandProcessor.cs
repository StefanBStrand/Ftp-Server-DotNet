namespace Group14.FtpServer
{
    /// <summary>
    /// Prosesses commands by rerouting them to the handlers.
    /// </summary>
    public interface IAsyncFtpCommandProcessor
    {
        /// <summary>
        /// Processes an FTP command and returns a response.
        /// </summary>
        /// <param name="command">The full command string from the client.</param>
        /// <param name="connection">The connection to the client.</param>
        /// <param name="session">The current session state.</param>
        /// <returns>FTP response code and message.</returns>
        public Task<string> ProcessCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session);
    }
}
