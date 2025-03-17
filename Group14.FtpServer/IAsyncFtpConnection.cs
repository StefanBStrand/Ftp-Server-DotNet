using System.Net.Sockets;

namespace Group14.FtpServer
{
    /// <summary>
    /// Represents a connection an FTP client.
    /// </summary>
    public interface IAsyncFtpConnection : IDisposable
    {
        /// <summary>
        /// Gets clients stream for the data transfer.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream();

        /// <summary>
        /// Reads a command from the client.
        /// </summary>
        /// <returns>The command sent by the client, or null if the connection is closed.</returns>
        Task<string> ReadCommandAsync();

        /// <summary>
        /// Sends a response to the client.
        /// </summary>
        /// <param name="response">The response to send</param>
        Task SendResponseAsync(string response);

        /// <summary>
        /// Closes the connection, releasing all resoruces.
        /// </summary>
        Task CloseAsync();
    }
}
