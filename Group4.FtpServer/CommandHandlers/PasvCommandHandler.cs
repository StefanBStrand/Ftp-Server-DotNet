using System.Net.Sockets;
using System.Net;

namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the PASV command to enter passive mode for data transfer.
    /// </summary>
    public class PasvCommandHandler : IAsyncFtpCommandHandler, IDataConnectionHandler
    {
        private readonly FtpServerOptions _serverOptions;
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string FailureResponsePrefix = "500 Failed to enter passive mode: ";
        private const string DefaultPassiveIp = "127.0.0.1";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "PASV";

        /// <summary>
        /// Initializes a new instance of the PasvCommandHandler class.
        /// </summary>
        /// <param name="serverOptions">The FTP server options containing configuration settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if serverOptions is null.</exception>
        public PasvCommandHandler(FtpServerOptions serverOptions)
        {
            if (serverOptions == null)
            {
                throw new ArgumentNullException(nameof(serverOptions), "The options provided cannot be null.");
            }
            _serverOptions = serverOptions;
        }

        /// <summary>
        /// Processes the PASV command to establish a passive mode data connection.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string with passive mode details or an error message.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
            {
                return Task.FromResult(NotAuthenticatedResponse);
            }

            try
            {
                if (session.DataListener != null)
                {
                    session.DataListener.Stop();
                    session.DataListener = null!;
                }

                IPAddress passiveIpAddress = _serverOptions.PasvIpAddress;
                if (passiveIpAddress == null || passiveIpAddress.Equals(IPAddress.Any))
                {
                    passiveIpAddress = IPAddress.Parse(DefaultPassiveIp);
                }

                session.DataListener = new TcpListener(passiveIpAddress, 0);
                session.DataListener.Start();

                int port = ((IPEndPoint)session.DataListener.LocalEndpoint).Port;

                string ipString = passiveIpAddress.ToString();
                string ipFormatted = ipString.Replace('.', ',');
                int portHigh = port / 256;
                int portLow = port % 256;

                return Task.FromResult($"227 Entering Passive Mode ({ipFormatted},{portHigh},{portLow}).");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"{FailureResponsePrefix}{ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the TCP client for the passive data connection.
        /// </summary>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>The TCP client for data transfer.</returns>
        /// <exception cref="InvalidOperationException">Thrown if passive mode is not initialized.</exception>
        public TcpClient GetDataClient(IFtpSession session)
        {
            if (session.DataListener == null)
            {
                throw new InvalidOperationException("Passive mode not initialized.");
            }

            return session.DataListener.AcceptTcpClient();
        }

        /// <summary>
        /// Closes the passive data channel if it is open.
        /// </summary>
        /// <param name="session">The current FTP session state.</param>
        public void CloseDataChannel(IFtpSession session)
        {
            if (session.DataListener != null)
            {
                session.DataListener.Stop();
                session.DataListener = null!;
            }
        }
    }
}