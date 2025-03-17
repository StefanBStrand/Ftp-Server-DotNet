namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the stor command to save a file on the server
    /// </summary>
    internal class StoreCommandHandler : IFtpCommandHandler
    {
        private readonly IBackendStorage _storage;
        private readonly IDataConnectionHandler _dataHandler;

        /// <summary>
        /// Initializes a new instance of the StoreCommandHandler.
        /// </summary>
        /// <param name="storage">The backend storage responsible for storing files.</param>
        /// <param name="dataHandler">The data connection handler used to receive file data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the storage or data handler is null.
        /// </exception>
        public StoreCommandHandler(IBackendStorage storage, IDataConnectionHandler dataHandler)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "Storage type can't be null.");

            if (dataHandler == null)
                throw new ArgumentNullException(nameof(dataHandler), "Data connection handler can't be null.");

            _dataHandler = dataHandler;
            _storage = storage;
        }
        public string HandleCommand(string command, IFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return "530 Please login with USER and PASS.";

            var parts = command.Split(' ', 2);
            if (parts.Length < 2)
                return "501 Syntax error in parameters.";

            var fileName = parts[1].Trim();
            var filePath = Path.Combine(session.CurrentDirectory, fileName).Replace('\\', '/');

            connection.SendResponse("150 Ready to receive data.");
            try
            {
                var dataClient = _dataHandler.GetDataClient(session);
                var data = ReadFileData(dataClient.GetStream()); // read data with the stream

                // close connection
                dataClient.Close();
                _dataHandler.CloseDataChannel(session);

                _storage.StoreFile(filePath, data); 
                return "226 File stored successfully";
            }
            catch (Exception ex)
            {
                _dataHandler.CloseDataChannel(session);
                return $"550 Failed to store file: {ex.Message}";
            }
        }

        private byte[] ReadFileData(Stream stream)
        {
            var buffer = new byte[8192]; // we need a buffer of some sort, starting with 8kbs
            var allData = new List<byte>();

            int bytesRead;
            // read from stream as long as there is something to read from
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++) // add read bytes to the list
                {
                    allData.Add(buffer[i]);
                }
            }

            return allData.ToArray(); // return data in bytes
        }
    }
}
