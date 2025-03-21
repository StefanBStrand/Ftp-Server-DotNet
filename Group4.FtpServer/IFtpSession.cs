using System.Net.Sockets;

namespace Group4.FtpServer
{
    /// <summary>
    /// Represents a session for an FTP client connection.
    /// </summary>
    public interface IFtpSession
    {
        /// <summary>
        /// Gets or sets whether the client is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the current working directory.
        /// </summary>
        public string CurrentDirectory { get; set; }

        /// <summary>
        /// Gets the storage backend type for file operations.
        /// </summary>
        public IBackendStorage Storage { get;}

        /// <summary>
        /// Gets or sets the username for the session
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the transfer type (ASCII or binary).
        /// </summary>
        string TransferType { get; set; }

        /// <summary>
        /// Gets or sets the data listener for passive mode
        /// </summary>
        TcpListener DataListener { get; set; } 
    }
}
