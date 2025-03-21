namespace Group4.FtpServer
{
    /// <summary>
    /// Defines a contract for an FTP server with asynchronous operations.
    /// </summary>
    public interface IFtpServer
    {
        /// <summary>
        /// Starts the FTP server asynchronously.
        /// </summary>
        /// <returns>A task that resolves to a message indicating the result of the operation.</returns>
        public Task<string> StartAsync();

        /// <summary>
        /// Stops the FTP server asynchronously.
        /// </summary>
        /// <returns>A task that resolves to a message indicating the result of the operation.</returns>
        public Task<string> StopAsync();
    }
}
