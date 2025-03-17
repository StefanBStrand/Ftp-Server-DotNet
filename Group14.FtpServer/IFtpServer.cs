namespace Group14.FtpServer
{
    /// <summary>
    /// Defines a contract for an FTP server
    /// </summary>
    public interface IFtpServer
    {
        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start();

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop();
    }
}
