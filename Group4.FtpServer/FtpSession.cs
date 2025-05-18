using System.Net.Sockets;

namespace Group4.FtpServer
{
    public class FtpSession : IFtpSession
    {
        /// <summary>
        /// Gets or sets whether the client is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the current working directory. Default is '/'
        /// </summary>
        public string CurrentDirectory { get; set; } = "/";

        /// <summary>
        /// Gets the storage backend type for file operations.
        /// </summary>
        public IBackendStorage Storage { get; }

        /// <summary>
        /// Gets or sets the username for the session.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets teh transfer type (ASCII or binary).
        /// </summary>
        public string TransferType { get; set; } = "I";

        /// <summary>
        /// Gets or sets the data listener for passive mode.
        /// </summary>
        public TcpListener DataListener { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of DefaultFtpSession with a storage backend.
        /// </summary>
        /// <param name="storage">The storage backend to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if storage is null.</exception>
        public FtpSession(IBackendStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "The storage type can't be null.");

            Storage = storage;
        }
    }
}
