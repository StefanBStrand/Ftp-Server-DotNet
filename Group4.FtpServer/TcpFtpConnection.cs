using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Group4.FtpServer
{
    /// <summary>
    /// Implements a FTP connection over TCP with optional support of TLS.
    /// </summary>
    public class TcpFtpConnection : IAsyncFtpConnection
    {
        private readonly TcpClient _tcpClient;
        private Stream _stream;
        private StreamReader _reader = null!;
        private StreamWriter _writer = null!;
        private bool _disposed;
        private readonly X509Certificate2? _certificate;
        private readonly bool _implicitTls;


        /// <summary>
        /// Initializes a new instance of the TcpFtpConnection class.
        /// </summary>
        /// <param name="tcpClient">The TCP client to use for the connection.</param>
        /// <param name="certificate">The TLS certificate to use, or null if TLS is disabled.</param>
        /// <param name="implicitTls">Indicates whether TLS should be applied immediately (true) or explicitly later (false). Defaults to false.</param>
        /// <exception cref="ArgumentNullException">Thrown if tcpClient is null.</exception>
        public TcpFtpConnection(TcpClient tcpClient, X509Certificate2? certificate = null, bool implicitTls = false)
        {
            // NOTE: this should ideally be split into two overloads based on TLS usage,
            // but doing so now would break existing implementations relying on this optional parameter.
            // Perhaps this constructor can be made obsolete in the future.

            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient), "The client cannot be null.");

            _tcpClient = tcpClient;
            _certificate = certificate;
            _implicitTls = implicitTls;
            _stream = _tcpClient.GetStream();

            if (_certificate != null && _implicitTls)
            {
                UpgradeToTlsImmediate();
            }

            InitializeStreams();
        }

        /// <summary>
        /// Initializes a new instance of the TcpFtpConnection class with a TLS certificate,
        /// but defers the TLS upgrade (AUTH TLS required).
        /// </summary>
        /// <param name="tcpClient">The TCP client to use.</param>
        /// <param name="certificate">The TLS certificate to use.</param>
        public TcpFtpConnection(TcpClient tcpClient, X509Certificate2 certificate)
            : this(tcpClient, certificate, false)
        { }

        private void InitializeStreams()
        {
            _reader = new StreamReader(_stream, Encoding.ASCII, leaveOpen: true);
            _writer = new StreamWriter(_stream, Encoding.ASCII, leaveOpen: true) { AutoFlush = true };
        }

        private void UpgradeToTlsImmediate()
        {
            var cert = _certificate!;
            try
            {
                var sslStream = new SslStream(_stream, false);
                sslStream.AuthenticateAsServer(cert);
                _stream = sslStream;
            }
            catch (AuthenticationException ex)
            {
                throw new InvalidOperationException("Failed to authenticate TLS connection.", ex);
            }
        }

        /// <summary>
        /// Upgrades the connection to TLS explicitly, typically after an AUTH TLS command.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if TLS is not configured or already active.</exception>
        public async Task UpgradeToTlsAsync()
        {
            if (_certificate == null)
                throw new InvalidOperationException("TLS cannot be enabled because no certificate was provided.");

            if (_stream is SslStream)
                throw new InvalidOperationException("Connection is already secured with TLS.");

            var sslStream = new SslStream(_stream, leaveInnerStreamOpen: true);
            await sslStream.AuthenticateAsServerAsync(_certificate);
            _stream = sslStream;
            InitializeStreams();
        }

        /// <summary>
        /// Gets clients stream for the data transfer.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream() 
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TcpFtpConnection));

            return _stream;
        }

        /// <summary>
        /// Reads a command from the client.
        /// </summary>
        /// <returns>The command sent by the client, or null if the connection is closed.</returns>
        public async Task<string> ReadCommandAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TcpFtpConnection), "The connection has been disposed.");

            try
            {
                var line = await _reader.ReadLineAsync();
                return line!;
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Failed to read command due to a network error.", ex);
            }
        }


        /// <summary>
        /// Sends a response to the client asynchronously.
        /// </summary>
        /// <param name="response">The response string to send.</param>
        /// <exception cref="ArgumentException">Thrown if response is null or empty.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if sending fails due to a network error.</exception>
        public async Task SendResponseAsync(string response)
        {
            if (string.IsNullOrEmpty(response))
                throw new ArgumentException("Response cannot be null or empty.", nameof(response));

            if (_disposed)
                throw new ObjectDisposedException(nameof(TcpFtpConnection), "The connection has been disposed.");

            if (!_tcpClient.Connected)
                return;

            try
            {
                await _writer.WriteLineAsync(response);
                await _writer.FlushAsync();
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Failed to send response due to a network error.", ex);
            }
        }

        /// <summary>
        /// Closes the connection, releasing all resoruces. Equivalent to Dispose().
        /// </summary>
        public async Task CloseAsync()
        {
            if (_disposed)
                return;

            if (_stream is SslStream sslStream)
            {
                try
                {
                    await sslStream.ShutdownAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error shutting down TLS: {ex}");
                }
            }

            _tcpClient?.Close();
            _disposed = true;
        }

        /// <summary>
        /// Releases the resources used by instance of the class.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _writer?.Dispose();
            _reader?.Dispose();
            _stream?.Dispose();
            _tcpClient?.Dispose();
            _disposed = true;
        }
    }
}
