namespace Group14.FtpServer
{
    /// <summary>
    /// Represents a connection an FTP client.
    /// </summary>
    public interface IFtpConnection : IDisposable
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
        public string ReadCommand();

        /// <summary>
        /// Sends a response to the client.
        /// </summary>
        /// <param name="response">The response to send</param>
        public void SendResponse(string response);

        /// <summary>
        /// Closes the connection, releasing all resoruces.
        /// </summary>
        public void Close();
    }
}
