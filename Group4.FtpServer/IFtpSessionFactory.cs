namespace Group4.FtpServer
{
    /// <summary>
    /// Defines a factory for creating a FTP client session.
    /// </summary>
    public interface IFtpSessionFactory
    {
        /// <summary>
        /// Creates a new FTP session for a client.
        /// </summary>
        /// <returns>A new instance of an FTP session.</returns>
        IFtpSession CreateSession();
    }
}
