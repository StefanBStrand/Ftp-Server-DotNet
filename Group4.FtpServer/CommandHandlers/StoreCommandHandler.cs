namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the STOR command to store a file on the server.
    /// </summary>
    public class StoreCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly IBackendStorage _storageBackend;
        private readonly IDataConnectionHandler _dataConnectionHandler;
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SyntaxErrorResponse = "501 Syntax error in parameters.";
        private const string ReadyToReceiveResponse = "150 Ready to receive data.";
        private const string SuccessResponse = "226 File stored successfully.";
        private const string FailureResponsePrefix = "550 Failed to store file: ";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "STOR";

        /// <summary>
        /// Initializes a new instance of the StoreCommandHandler class.
        /// </summary>
        /// <param name="storageBackend">The storage backend used to store the file.</param>
        /// <param name="dataConnectionHandler">The handler managing data connections for file transfers.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public StoreCommandHandler(IBackendStorage storageBackend, IDataConnectionHandler dataConnectionHandler)
        {
            if (storageBackend == null)
                throw new ArgumentNullException(nameof(storageBackend), "Storage type can't be null.");

            if (dataConnectionHandler == null)
                throw new ArgumentNullException(nameof(dataConnectionHandler), "Data connection handler can't be null.");

            _dataConnectionHandler = dataConnectionHandler;
            _storageBackend = storageBackend;
        }

        /// <summary>
        /// Processes the STOR command to receive a file over the data connection and store it.
        /// </summary>
        /// <param name="command">The full command string received from the client (e.g., "STOR file.txt").</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the result of the operation.</returns>
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

            await connection.SendResponseAsync(ReadyToReceiveResponse);
            try
            {
                using var dataClient = _dataConnectionHandler.GetDataClient(session);
                using var stream = dataClient.GetStream();

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                byte[] fileData = memoryStream.ToArray();

                await _storageBackend.StoreFileAsync(filePath, fileData);
                _dataConnectionHandler.CloseDataChannel(session);
                return SuccessResponse;
            }
            catch (Exception ex)
            {
                _dataConnectionHandler.CloseDataChannel(session);
                return $"{FailureResponsePrefix}{ex.Message}";
            }
        }
    }
}
