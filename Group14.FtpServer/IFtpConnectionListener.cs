namespace Group14.FtpServer
{
    /// <summary>
    /// Represents a listener for the incoming FTP connections.
    /// </summary>
    public interface IFtpConnectionListener
    {
        /// <summary>
        /// Accepts a new clients connection.
        /// </summary>
        /// <returns>The accepted FTP connection.</returns>
        public IAsyncFtpConnection AcceptConnection();

        /// <summary>
        /// Starts listening for the connections.
        /// </summary>
        public void Start();

        /// <summary>
        /// Stops listening for connections.
        /// </summary>
        public void Stop();
    }
}
