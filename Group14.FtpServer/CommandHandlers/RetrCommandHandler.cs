using System.Net.Sockets;

namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the RETR command to retrieve a file.
    /// </summary>
    internal class RetrCommandHandler : IFtpCommandHandler
    {
        private readonly IBackendStorage _storage;
        private readonly IDataConnectionHandler _dataHandler;

        /// <summary>
        /// Initializes a new instance of the RetrCommandHandler class.
        /// </summary>
        /// <param name="storage">The backend storage responsible for retrieving file from.</param>
        /// <param name="dataHandler">The data handler used to retrieve the file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the storage or datahandler is null.
        /// </exception>
        public RetrCommandHandler(IBackendStorage storage, IDataConnectionHandler dataHandler)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "Storage type can't be null.");

            if (dataHandler == null)
                throw new ArgumentNullException(nameof(dataHandler), "Data handler can't be null.");

            _storage = storage;
            _dataHandler = dataHandler;
        }

        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public string HandleCommand(string command, IFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return "530 Please login with USER and PASS.";

            var parts = command.Split(' ', 2);
            if (parts.Length < 2)
                return "501 Syntax error in parameters.";

            // get filename and path
            var fileName = parts[1].Trim();
            var filePath = Path.Combine(session.CurrentDirectory, fileName).Replace('\\', '/');

            try
            {
                byte[] retrievedData = _storage.RetrieveFile(filePath); // get data from storage
                connection.SendResponse("150 Opening data connection for file transfer.");

                TcpClient dataClient = _dataHandler.GetDataClient(session); // use the data connection from pasv
                using (var stream = dataClient.GetStream())
                {
                    stream.Write(retrievedData, 0, retrievedData.Length);
                }

                dataClient.Close();
                _dataHandler.CloseDataChannel(session);

                return "226 Transfer complete.";
            }
            catch (Exception)
            {
                _dataHandler.CloseDataChannel(session);
                return "550 File unavailable or retrieval failed.";
            }
        }
    }
}
