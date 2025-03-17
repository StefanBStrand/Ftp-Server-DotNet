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
    public class TcpFtpConnection : IFtpConnection // sealed class?
    {
        private readonly TcpClient _tcpClient;
        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;
        private Stream _stream;
        private bool _disposed;

        /// <summary>
        /// Initializes a new FTP connection over TCP.
        /// </summary>
        /// <param name="tcpClient">The TCP client to use.</param>
        /// <param name="certificate">The certificate to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if TCP client is null.</exception>
        public TcpFtpConnection(TcpClient tcpClient, X509Certificate2 certificate)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient), "The client can't be null.");

            _tcpClient = tcpClient;
            if (certificate != null) 
            {
                _stream = tcpClient.GetStream();
                try
                {
                    SslStream sslStream = new SslStream(_stream, false);
                    sslStream.AuthenticateAsServer(certificate);
                    _stream = sslStream;
                }
                catch (AuthenticationException e)
                {
                    throw new InvalidOperationException("Failed to authenticate TLS connection.", e);
                }
            }
            else
            {
                _stream = _tcpClient.GetStream();
            }
            _reader = new StreamReader(_stream, Encoding.ASCII);
            _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };
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
        public string ReadCommand()
        {
            if (_stream == null)
                throw new ObjectDisposedException(nameof(TcpFtpConnection), "The stream has been disposed of.");

            try
            {
                return _reader.ReadLine();
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Failed to read command due to a network error.", e);
            }
        }

        /// <summary>
        /// Sends a response to the client.
        /// </summary>
        /// <param name="response">The response to be sent back to the client.</param>
        /// <exception cref="ArgumentException">Thrown if response is empty or null.</exception>
        public void SendResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                throw new ArgumentException(nameof(response), "The response can't be null or empty.");

            if (_stream == null)
                throw new ObjectDisposedException(nameof(TcpFtpConnection), "The stream has been disposed.");

            if (!_tcpClient.Connected)
                return;

            try
            {
                _writer.WriteLine(response);
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Failed to send response due to a network error.", e);
            }
        }

        /// <summary>
        /// Closes the connection, releasing all resoruces. Equivalent to Dispose().
        /// </summary>
        public void Close()
            // Consider providing a Close() method in addition to the Dispose() method if close is standard
            // terminology in the area.
        {
            Dispose();
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
