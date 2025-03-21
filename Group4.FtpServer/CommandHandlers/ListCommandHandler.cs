using System.Text;

namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the LIST command to retrieve and send directory contents to the client.
    /// </summary>
    public class ListCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly IBackendStorage _storage;
        private readonly IDataConnectionHandler _dataConnectionHandler;
        private readonly IListFormatter _formatter;
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string OpeningResponse = "150 Here is the directory listing";
        private const string SuccessResponse = "226 Directory sending ok";
        private const string FailureResponsePrefix = "550 Failed to list directory: ";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "LIST";

        /// <summary>
        /// Initializes a new instance of the ListCommandHandler class.
        /// </summary>
        /// <param name="storage">The backend storage used to retrieve directory contents.</param>
        /// <param name="dataConnectionHandler">The handler managing data connections for file transfers.</param>
        /// <param name="formatter">The formatter used to format directory listings.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public ListCommandHandler(IBackendStorage storage, IDataConnectionHandler dataConnectionHandler, IListFormatter formatter)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "Storage can't be null.");

            if (dataConnectionHandler == null)
                throw new ArgumentNullException(nameof(dataConnectionHandler), "Data connection handler can't be null.");

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter), "Formatter can't be null.");

            _storage = storage;
            _dataConnectionHandler = dataConnectionHandler;
            _formatter = formatter;
        }

        /// <summary>
        /// Processes the LIST command to send directory contents to the client over a data connection.
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

            try
            {
                var files = await _storage.ListAllFilesAsync(session.CurrentDirectory);
                var responseLines = files.Select(file => _formatter.FormatFileItem(file)).ToList();
                var response = string.Join("\r\n", responseLines);

                await connection.SendResponseAsync(OpeningResponse);

                using var dataClient = _dataConnectionHandler.GetDataClient(session);
                using var stream = dataClient.GetStream();
                using var writer = new StreamWriter(stream, Encoding.ASCII);

                await writer.WriteAsync(response);
                await writer.FlushAsync();

                _dataConnectionHandler.CloseDataChannel(session);
                return SuccessResponse;
            }
            catch (Exception e)
            {
                _dataConnectionHandler.CloseDataChannel(session);
                return $"{FailureResponsePrefix}{e.Message}";
            }
        }
    }
}