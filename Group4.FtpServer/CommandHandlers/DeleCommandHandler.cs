namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the DELE command to delete a specified file from the storage backend.
    /// </summary>
    public class DeleCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly IBackendStorage _storageBackend;
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SyntaxErrorResponse = "501 Syntax error in parameters.";
        private const string SuccessResponse = "250 File deleted successfully.";
        private const string FailureResponse = "550 File not found or deletion failed.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "DELE";

        /// <summary>
        /// Initializes a new instance of the DeleCommandHandler class.
        /// </summary>
        /// <param name="storageBackend">The storage backend used to perform file deletion.</param>
        /// <exception cref="ArgumentNullException">Thrown if storageBackend is null.</exception>
        public DeleCommandHandler(IBackendStorage storageBackend)
        {
            if (storageBackend == null)
                throw new ArgumentNullException(nameof(storageBackend), "Storage type can't be null.");

            _storageBackend = storageBackend;
        }

        /// <summary>
        /// Processes the DELE command to remove a file from the storage backend.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the result of the deletion operation.</returns>
        public async Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
            {
                return NotAuthenticatedResponse;
            }

            var commandArguments = command.Split(' ', 2);
            if (commandArguments.Length < 2)
            {
                return SyntaxErrorResponse;
            }

            var targetFileName = commandArguments[1].Trim();
            var filePath = Path.Combine(session.CurrentDirectory, targetFileName).Replace('\\', '/');
            var isDeleted = await _storageBackend.DeleteFileAsync(filePath);

            return isDeleted ? SuccessResponse : FailureResponse;
        }
    }
}
