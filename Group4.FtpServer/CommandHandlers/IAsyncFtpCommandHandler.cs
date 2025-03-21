namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Defines the contract for handling FTP commands asynchronously.
    /// </summary>
    public interface IAsyncFtpCommandHandler
    {
        /// <summary>
        /// Gets the FTP command string that this handler processes.
        /// </summary>
        public string Command { get; }


        /// <summary>
        /// Processes the specified FTP command and returns a response.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A task that represents the asynchronous operation, containing the FTP response string.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session);
    }
}
