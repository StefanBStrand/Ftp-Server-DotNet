using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Group4.FtpServer
{
    /// <summary>
    /// Represents the options of which the FTP server uses.
    /// </summary>
    public class FtpServerOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public FtpServerOptions() { }

        /// <summary>
        /// Gets or sets the IP address the server listens on. Defaults to IPAddress.Any.
        /// </summary>
        public IPAddress IpAddress { get; set; } = IPAddress.Any;

        /// <summary>
        /// Gets or sets the port number the server listens on. Defaults to 21.
        /// </summary>
        public int Port { get; set; } = 21;

        /// <summary>
        /// Gets or sets the certificate for TLS encryption. Null if TLS is disabled.
        /// </summary>
        public X509Certificate2? Certificate { get; set; }

        /// <summary>
        /// Gets or sets whether TLS encryption is enabled. Defaults to false.
        /// </summary>
        public bool EnableTls { get; set; } = false;

        /// <summary>
        /// Gets or sets the root path for file storage. Defaults to the current directory.
        /// Null is default indicating no local storage is enabled.
        /// </summary>
        public string? RootPath { get; set; } = null;

        /// <summary>
        /// Gets or sets the IP address advertised in PASV responses. Defaults to the listener IP.
        /// </summary>
        public IPAddress PasvIpAddress { get; set; } = IPAddress.Any;
    }
}
