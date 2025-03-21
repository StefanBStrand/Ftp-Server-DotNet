using System.Net.Sockets;

namespace Group4.FtpServer
{
    /// <summary>
    /// Defines a contract for handling data connections.
    /// </summary>
    public interface IDataConnectionHandler
    {
        /// <summary>
        /// Gets the TCP client for the data connection.
        /// </summary>
        /// <param name="session">The current session state.</param>
        /// <returns>The TCP client for data transfer.</returns>
        public TcpClient GetDataClient(IFtpSession session);

        /// <summary>
        /// Closes the data channel if open.
        /// </summary>
        /// <param name="session">The current session state.</param>
        public void CloseDataChannel(IFtpSession session);
    }
}
