namespace Group4.FtpServer
{
    /// <summary>
    /// Creates instances of FTP sessions using a specified or default backend storage.
    /// </summary>
    public class DefaultFtpSessionFactory : IFtpSessionFactory
    {
        private readonly IBackendStorage _storageBackend;

        /// <summary>
        /// Initializes a new instance of the DefaultFtpSessionFactory class with a specified backend storage.
        /// </summary>
        /// <param name="storageBackend">The backend storage to use for FTP sessions.</param>
        /// <exception cref="ArgumentNullException">Thrown storageBackend null.</exception>
        public DefaultFtpSessionFactory(IBackendStorage storageBackend)
        {
            if (storageBackend == null)
                throw new ArgumentNullException(nameof(storageBackend), "The backend storage can't be null.");

            _storageBackend = storageBackend;
        }

        /// <summary>
        /// Initializes a new default FTP session factory with a local file storage backend type.
        /// </summary>
        public DefaultFtpSessionFactory()
            : this(new LocalFileStorage()) { }

        /// <summary>
        /// Creates a new default session with a specified backend storage type.
        /// </summary>
        /// <returns>Returns a new instance of a default FTP session.</returns>
        public IFtpSession CreateSession()
        {
            return new FtpSession(_storageBackend);
        }
    }
}
