namespace Group14.FtpServer
{
    /// <summary>
    /// Defines a contract for an FTP server
    /// </summary>
    public interface AsyncIFtpServer
    {
        /// <summary>
        /// Starts the server.
        /// </summary>
        Task<string> Start();

        /// <summary>
        /// Stops the server.
        /// </summary>
        Task<string> Stop();
    }
}
