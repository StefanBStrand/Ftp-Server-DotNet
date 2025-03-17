using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Group14.FtpServer.Handlers
{

    /// <summary>
    /// Represents an FTP connection listener using TCP with the 
    /// optional support of TLS.
    /// </summary>
    public class TcpConnectionListener : IFtpConnectionListener // should this also be selead?
    {
        private readonly TcpListener _listener;
        private readonly FtpServerOptions _options;
        private bool _isRunning;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new TCP listener with specific server options and a specific logger instance.
        /// </summary>
        /// <param name="options">The options to base the listener on.</param>
        /// <param name="logger">A specific logger used to log information and errors.</param>
        /// <exception cref="ArgumentNullException">Throw if server options is null.</exception>
        public TcpConnectionListener(FtpServerOptions options, ILogger logger)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "Server options can't be null.");

            _options = options;
            _isRunning = false;
            _listener = new TcpListener(options.IpAddress, options.Port);
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new TCP listener with specific server options and without a specific logger instance.
        /// </summary>
        /// <param name="options">The options to base the listener on.</param>
        /// <exception cref="ArgumentNullException">Throw if server options is null.</exception>
        public TcpConnectionListener(FtpServerOptions options)
        : this (options, null) { }

        /// <summary>
        /// Starts the listener.
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            _listener.Start();
            _isRunning = true;
            _logger?.LogInformation("TCP Listener has started on {Address}:{Port}", _options.IpAddress, _options.Port);
        }

        /// <summary>
        /// Stops the listener.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) 
                return;

            _listener.Stop();
            _isRunning = false;
            _logger?.LogInformation("TCP listener has stopped.");
        }

        /// <summary>
        /// Accepts a new TCP connection.
        /// </summary>
        /// <returns>
        /// Returns a new TcpFtpConnection with or without a certificate
        /// based if server options has TLS enabled.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if listener is not running.</exception>
        public IAsyncFtpConnection AcceptConnection()
        {
            if (!_isRunning)
                throw new InvalidOperationException("Listener must be running before accepting any connections.");

            try
            {
                TcpClient client = _listener.AcceptTcpClient();

                if (_options.EnableTls && _options.Certificate == null)
                    throw new InvalidOperationException("TLS is enabled but no certificate is provided.");
                return CreateConnection(client);

            }
            catch (SocketException e)
            {
                _logger?.LogError(e, "Socket error while accepting the connection. ");
                throw;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Unexpected error while accepting connection.");
                throw;
            }
        }

        protected IAsyncFtpConnection CreateConnection(TcpClient client)
        {
            return new AsyncTcpFtpConnection(client, _options.EnableTls ? _options.Certificate : null, implicitTls: false);
        }
    }
}
