using System.Net.Sockets;

namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the stor command to save a file on the server
    /// </summary>
    public class StoreCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly IBackendStorage _storage;
        private readonly IDataConnectionHandler _dataHandler;
        public string Command => "STOR";

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

        public async Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return "530 Please login with USER and PASS.";

            var parts = command.Split(' ', 2);
            if (parts.Length < 2)
                return "501 Syntax error in parameters.";

            var fileName = parts[1].Trim();
            var filePath = Path.Combine(session.CurrentDirectory, fileName).Replace('\\', '/');

            connection.SendResponseAsync("150 Ready to receive data.");
            try
            {
                TcpClient dataClient = _dataHandler.GetDataClient(session);
                byte[] data = await ReadFileDataAsync(dataClient.GetStream());
                dataClient.Close();
                _dataHandler.CloseDataChannel(session);

                await _storage.StoreFileAsync(filePath, data);
                return "226 File stored successfully";
            }
            catch (Exception ex)
            {
                _dataHandler.CloseDataChannel(session);
                return $"550 Failed to store file: {ex.Message}";
            }
        }


        private async Task<byte[]> ReadFileDataAsync(Stream stream)
        {
            var buffer = new byte[8192];
            var allData = new List<byte>();

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    allData.Add(buffer[i]);
                }
            }

            return allData.ToArray();
        }
    }
}
