namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the DELE command to delete a file.
    /// </summary>
    public class DeleCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly IBackendStorage _storage;
        public string Command => "DELE";

        /// <summary>
        /// Initializes a new DELE command handler.
        /// </summary>
        /// <param name="storage">The storage type to execute the DELE command to.</param>
        /// <exception cref="ArgumentNullException">Thrown if storage type is null</exception>
        public DeleCommandHandler(IBackendStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "Storage type can't be null.");

            _storage = storage;
        }

        /// <summary>
        /// Processes an ftp command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public async Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return "530 Please login with USER and PASS.";

            var parts = command.Split(' ', 2);
            if (parts.Length < 2)
                return "501 Syntax error in parameters.";

            var fileName = parts[1].Trim();
            var filePath = Path.Combine(session.CurrentDirectory, fileName);
            bool deleted = await _storage.DeleteFileAsync(filePath); 
            return deleted ? "250 File deleted successfully." : "550 File not found or deletion has failed.";
        }
    }
}
