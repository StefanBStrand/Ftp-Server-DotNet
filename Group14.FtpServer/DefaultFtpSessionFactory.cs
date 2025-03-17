namespace Group14.FtpServer
{
    /// <summary>
    /// Default implementation of an FTP session factory.
    /// </summary>
    public class DefaultFtpSessionFactory : IFtpSessionFactory
    {
        private readonly IBackendStorage _backendStorage;

        /// <summary>
        /// Initializes a new default FTP session factory with a specified backend storage type.
        /// </summary>
        /// <param name="backendStorage">The storage backend to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if provided backend storage is null.</exception>
        public DefaultFtpSessionFactory(IBackendStorage backendStorage)
        {
            if (backendStorage == null)
                throw new ArgumentNullException(nameof(backendStorage), "The backend storage type can't be null.");

            _backendStorage = backendStorage;
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
            return new DefaultFtpSession(_backendStorage);
        }
    }
}
