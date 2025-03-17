using Group14.FtpServer.Handlers;
using System.Net.Sockets;

namespace Group14.FtpServer.Tests.ConnectionTests
{
    [TestClass]
    public class TcpConnectionListenerTests
    {

        [TestMethod]
        public void AcceptConnection_WhenServerIsRunning_ReturnsValidConnection()
        {
            // Arrange
            var options = new FtpServerOptions { Port = 2121 };
            var listener = new TcpConnectionListener(options);
            listener.Start();

            // Act
            var client = new TcpClient("127.0.0.1", 2121);
            var connection = listener.AcceptConnection();

            // Assert
            Assert.IsNotNull(connection);
            Assert.IsInstanceOfType(connection, typeof(TcpConnectionListener));
        }
    }
}
