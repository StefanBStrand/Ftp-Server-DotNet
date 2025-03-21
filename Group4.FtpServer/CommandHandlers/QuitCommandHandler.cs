namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the QUIT command to terminate the FTP session.
    /// </summary>
    public class QuitCommandHandler : IAsyncFtpCommandHandler
    {
        private const string GoodbyeResponse = "221 Goodbye";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "QUIT";

        /// <summary>
        /// Processes the QUIT command to close the FTP session.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the session is closing.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            connection.SendResponseAsync(GoodbyeResponse);
            connection.CloseAsync();
            return Task.FromResult(GoodbyeResponse);
        }
    }
}