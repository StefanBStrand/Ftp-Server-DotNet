using System.Net.Sockets;
using System.Text;

namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the LIST command to show directory contents.
    /// </summary>
    public class ListCommandHandler : IAsyncFtpCommandHandler
    {
        private readonly IBackendStorage _storage;
        private readonly IDataConnectionHandler _dataHandler;
        private readonly IListFormatter _formatter;
        public string Command => "LIST";

        /// <summary>
        /// Initializes a new instance of the ListCommandHandler.
        /// </summary>
        /// <param name="storage">The backend storage for retrieving directory contents.</param>
        /// <param name="pasvHandler">The PASV command handler for managing data connections.</param>
        /// <param name="formatter">The formatter used to format directory listings.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if storage, pasvHandler, or formatter is null.
        /// </exception>
        public ListCommandHandler(IBackendStorage storage, PasvCommandHandler pasvHandler, IListFormatter formatter)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "Storage can't be null.");

            if (pasvHandler == null)
                throw new ArgumentNullException(nameof(pasvHandler), "Pasv handler can't be null.");

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter), "Formatter can't be null.");

            _storage = storage;
            _dataHandler = pasvHandler;
            _formatter = formatter;
        }

        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public async Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return "530 Please login with USER and PASS.";

            try
            {
                IEnumerable<FileItem> files = await _storage.ListAllFilesAsync(session.CurrentDirectory);
                List<string> responseLines = new List<string>();

                foreach (FileItem file in files)
                {
                    string formattedLine = _formatter.FormatFileItem(file);
                    responseLines.Add(formattedLine);
                }

                string response = string.Join("\r\n", responseLines);

                connection.SendResponseAsync("150 Here is the directory listing");

                TcpClient dataClient = _dataHandler.GetDataClient(session);
                using (Stream stream = dataClient.GetStream())
                using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
                {
                    await writer.WriteAsync(response);
                    await writer.FlushAsync();
                }
                dataClient.Close();

                if (_dataHandler is PasvCommandHandler pasvHandler)
                {
                    pasvHandler.CloseDataChannel(session);
                }
                return "226 Directory sending ok";
            }
            catch (Exception ex)
            {
                if (_dataHandler is PasvCommandHandler pasvHandler)
                {
                    pasvHandler.CloseDataChannel(session);
                }
                return $"550 Failed to list directory: {ex.Message}";
            }
        }
    }
}