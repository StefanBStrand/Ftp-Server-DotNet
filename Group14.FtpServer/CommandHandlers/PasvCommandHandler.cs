using System.Net.Sockets;
using System.Net;

namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the PASV command to enter passive mode for data transfer.
    /// </summary>
    public class PasvCommandHandler : IAsyncFtpCommandHandler, IDataConnectionHandler
    {
        private readonly FtpServerOptions _options;
        public string Command => "PASV";

        /// <summary>
        /// Initializes a new instance of the PasvCommandHandler class.
        /// </summary>
        /// <param name="options">The FTP server options containing configuration settings.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if options is null.
        /// </exception>
        public PasvCommandHandler(FtpServerOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "The options provided can't be null");
            _options = options;
        }

        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return Task.FromResult("530 Please login with USER and PASS.");

            try
            {
                if (session.DataListener != null)
                {
                    session.DataListener.Stop();
                    session.DataListener = null;
                }

                IPAddress pasvIp = _options.PasvIpAddress;
                if (pasvIp == null || pasvIp.Equals(IPAddress.Any))
                {
                    pasvIp = IPAddress.Parse("127.0.0.1");
                }

                session.DataListener = new TcpListener(pasvIp, 0);
                session.DataListener.Start();

                int port = ((IPEndPoint)session.DataListener.LocalEndpoint).Port;

                string ipString = pasvIp.ToString();
                string ipFormatted = ipString.Replace('.', ',');
                int portHigh = port / 256;
                int portLow = port % 256;

                return Task.FromResult($"227 Entering Passive Mode ({ipFormatted},{portHigh},{portLow}).");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"500 Failed to enter passive mode: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the TCP client for the data connection.
        /// </summary>
        /// <param name="session">The current session state.</param>
        /// <returns>The tcp client for data transfer.</returns>
        /// <exception cref="InvalidOperationException">Thrown if passive mode is not initialized.</exception>
        public TcpClient GetDataClient(IFtpSession session)
        {
            if (session.DataListener == null)
                throw new InvalidOperationException("Passive mode not initialized.");

            return session.DataListener.AcceptTcpClient();
        }

        /// <summary>
        /// Closes the data channel if open.
        /// </summary>
        /// <param name="session">the current session state.</param>
        public void CloseDataChannel(IFtpSession session)
        {
            if (session.DataListener != null)
            {
                session.DataListener.Stop();
                session.DataListener = null;
            }
        }
    }
}