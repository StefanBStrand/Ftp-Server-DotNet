namespace Group4.FtpServer
{
    /// <summary>
    /// Represents a listener for the incoming FTP connections.
    /// </summary>
    public interface IFtpConnectionListener
    {
        /// <summary>
        /// Starts listening for the connections.
        /// </summary>
        public void Start();

        /// <summary>
        /// Stops listening for connections.
        /// </summary>
        public void Stop();

        /// <summary>
        /// Accepts an incoming FTP connection.
        /// </summary>
        /// <returns>A new instance of IAsyncFtpConnection representing the accepted connection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the listener is not running.</exception>
        public IAsyncFtpConnection AcceptConnection();

    }
}
