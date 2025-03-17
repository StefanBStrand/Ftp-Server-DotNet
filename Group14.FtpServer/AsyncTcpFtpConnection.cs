using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Group14.FtpServer
{
    /// <summary>
    /// Implements a FTP connection over TCP with optional support of TLS.
    /// </summary>
    public class AsyncTcpFtpConnection : IAsyncFtpConnection
    {
        private readonly TcpClient _tcpClient;
        private Stream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _disposed;
        private readonly X509Certificate2 _certificate;
        private readonly bool _implicitTls;


        /// <summary>
        /// Initializes a new instance of the TcpFtpConnection class.
        /// </summary>
        /// <param name="tcpClient">The TCP client to use for the connection.</param>
        /// <param name="certificate">The TLS certificate to use (null if TLS is disabled).</param>
        /// <param name="implicitTls">
        /// Indicates whether TLS should be started immediately (implicit) or later via explicit upgrade (AUTH TLS).
        /// Default is false (explicit TLS).
        /// </param>
        public AsyncTcpFtpConnection(TcpClient tcpClient, X509Certificate2 certificate, bool implicitTls = false)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient), "The client cannot be null.");

            _tcpClient = tcpClient;
            _certificate = certificate;
            _implicitTls = implicitTls;
            _stream = _tcpClient.GetStream();

            if (_certificate != null && _implicitTls)
            {
                try
                {
                    var sslStream = new SslStream(_stream, false);
                    sslStream.AuthenticateAsServer(_certificate);
                    _stream = sslStream;
                }
                catch (AuthenticationException ex)
                {
                    throw new InvalidOperationException("Failed to authenticate TLS connection.", ex);
                }
            }

            _reader = new StreamReader(_stream, Encoding.ASCII);
            _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };
        }

        /// <summary>
        /// Upgrades the current connection to TLS.
        /// This is used for explicit TLS when the client sends the AUTH TLS command.
        /// </summary>
        public async Task UpgradeToTlsAsync()
        {
            if (_certificate == null)
                throw new InvalidOperationException("TLS cannot be enabled because no certificate was provided.");

            if (_stream is SslStream)
                throw new InvalidOperationException("Connection is already secured with TLS.");

            var sslStream = new SslStream(_stream, leaveInnerStreamOpen: true);
            await sslStream.AuthenticateAsServerAsync(_certificate, clientCertificateRequired: false,
                                                        enabledSslProtocols: SslProtocols.Tls12,
                                                        checkCertificateRevocation: false);
            _stream = sslStream;
            _reader = new StreamReader(_stream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
            _writer = new StreamWriter(_stream, Encoding.ASCII, bufferSize: 1024, leaveOpen: true) { AutoFlush = true };
        }

        /// <summary>
        /// Gets clients stream for the data transfer.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream() 
        {  
            return _stream; 
        }

        /// <summary>
        /// Reads a command from the client.
        /// </summary>
        /// <returns>The command sent by the client, or null if the connection is closed.</returns>
        public async Task<string> ReadCommandAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncTcpFtpConnection), "The connection has been disposed.");

            try
            {
                return await _reader.ReadLineAsync();
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Failed to read command due to a network error.", ex);
            }
        }


        /// <summary>
        /// Sends a response to the client.
        /// </summary>
        /// <param name="response">The response string to send.</param>
        public async Task SendResponseAsync(string response)
        {
            if (string.IsNullOrEmpty(response))
                throw new ArgumentException("Response cannot be null or empty.", nameof(response));

            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncTcpFtpConnection), "The connection has been disposed.");

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
            // Consider providing a Close() method in addition to the Dispose() method if close is standard
                                            // terminology in the area.
        {
            if (_stream is SslStream sslStream)
            {
                try
                {
                    await sslStream.ShutdownAsync(); 
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
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
            _writer?.Dispose();
            _reader?.Dispose();
            _stream?.Dispose();
            _tcpClient?.Dispose();
            _disposed = true;
        }
    }
}
