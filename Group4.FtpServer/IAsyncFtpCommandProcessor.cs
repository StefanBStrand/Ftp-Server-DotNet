namespace Group4.FtpServer
{
    /// <summary>
    /// Prosesses commands by rerouting them to the handlers.
    /// </summary>
    public interface IAsyncFtpCommandProcessor
    {
        /// <summary>
        /// Processes an FTP command and returns the response.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The connection to the client.</param>
        /// <param name="session">The current session state.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the FTP response code and message.</returns>
        public Task<string> ProcessCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session);
    }
}
