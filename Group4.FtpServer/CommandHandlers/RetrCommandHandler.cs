namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the RETR command to retrieve a file from the server.
    /// </summary>
    public class RetrCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly IBackendStorage _storageBackend;
        private readonly IDataConnectionHandler _dataConnectionHandler;
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string SyntaxErrorResponse = "501 Syntax error in parameters.";
        private const string OpeningConnectionResponse = "150 Opening data connection for file transfer.";
        private const string TransferCompleteResponse = "226 Transfer complete.";
        private const string FailureResponse = "550 File unavailable or retrieval failed.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "RETR";


        /// <summary>
        /// Initializes a new instance of the RetrCommandHandler class.
        /// </summary>
        /// <param name="storageBackend">The storage backend used to retrieve the file.</param>
        /// <param name="dataConnectionHandler">The handler managing data connections for file transfers.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public RetrCommandHandler(IBackendStorage storageBackend, IDataConnectionHandler dataConnectionHandler)
        {
            if (storageBackend == null)
                throw new ArgumentNullException(nameof(storageBackend), "Storage backend can't be null.");

            if (dataConnectionHandler == null)
                throw new ArgumentNullException(nameof(dataConnectionHandler), "Data connection handler can't be null.");

            _storageBackend = storageBackend;
            _dataConnectionHandler = dataConnectionHandler;
        }

        /// <summary>
        /// Processes the RETR command to retrieve a file and send it over the data connection.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
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

            try
            {
                byte[] fileData = await _storageBackend.RetrieveFileAsync(filePath);
                await connection.SendResponseAsync(OpeningConnectionResponse);

                using var dataClient = _dataConnectionHandler.GetDataClient(session);
                using var stream = dataClient.GetStream();

                await stream.WriteAsync(fileData, 0, fileData.Length);
                await stream.FlushAsync();

                _dataConnectionHandler.CloseDataChannel(session);
                return TransferCompleteResponse;
            }

            catch (Exception)
            {
                _dataConnectionHandler.CloseDataChannel(session);
                return FailureResponse;
            }
        }
    }
}
