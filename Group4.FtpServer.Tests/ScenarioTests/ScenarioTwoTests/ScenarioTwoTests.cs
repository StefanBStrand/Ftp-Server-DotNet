using System.Net.Sockets;
using System.Text;
using Group4.FtpServer.Handlers;
using Microsoft.Extensions.Logging;

namespace Group4.FtpServer.Tests.ScenarioTests.ScenarioTwoTests
{
    [TestClass]
    public class ScenarioTwoTests
    {
        private FtpServer _server = null!;
        private TestLogger _logger = null!;

        [TestInitialize]
        public void Setup()
        {
            var storage = new InMemoryDatabaseStorage();
            var authProvider = new SimpleAuthenticationProvider();
            var listFormatter = new UnixListFormatter();
            var options = new FtpServerOptions { Port = 2121, EnableTls = false }; // NOTE - No TLS, Fixed port.
            _logger = new TestLogger();
            var commandProcessor = new FtpCommandProcessor(storage, authProvider, listFormatter, options, _logger);
            var listener = new TcpConnectionListener(options);
            var sessionFactory = new DefaultFtpSessionFactory(storage);
            _server = new FtpServer(listener, commandProcessor, _logger, sessionFactory);
            _server.StartAsync().Wait();
        }

        [TestCleanup]
        public void Teardown()
        {
            _server.StopAsync().Wait();
        }

        [TestMethod]
        public async Task ScenarioTwo_BasicWorkflowWithoutTls_Success()
        {
            using var client = new TcpClient("localhost", 2121);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            Assert.AreEqual("220 Welcome to Group-4 FTP server.", await reader.ReadLineAsync());
            await writer.WriteLineAsync("USER test");
            Assert.AreEqual("331 Password required", await reader.ReadLineAsync());
            await writer.WriteLineAsync("PASS 1234");
            Assert.AreEqual("230 User logged in.", await reader.ReadLineAsync());

            await writer.WriteLineAsync("PASV");
            string response = (await reader.ReadLineAsync())!;
            int dataPort = GetDataPort(response);
            await writer.WriteLineAsync("STOR testfile.txt");
            Assert.AreEqual("150 Ready to receive data.", await reader.ReadLineAsync());
            using (var dataClient = new TcpClient("localhost", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII) { AutoFlush = true })
            {
                await dataWriter.WriteAsync("Hello, this is a test file.");
            }
            Assert.AreEqual("226 File stored successfully.", await reader.ReadLineAsync());

            await writer.WriteLineAsync("QUIT");
            Assert.AreEqual("221 Goodbye", await reader.ReadLineAsync());

            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: USER test"), "USER command not logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: PASS 1234"), "PASS command not logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: PASV"), "PASV command not logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: STOR testfile.txt"), "STOR command not logged.");
        }

        // helper method to get data-port from PASV respoonse
        // NOTE: The response is supposed t contain 6 numbers in parantheses, where
        // 5/6 numbers create the port
        private int GetDataPort(string response)
        {
            // example of how a response can look like
            // "227 Entering Passive Mode (127,0,0,1,45,229)"

            int start = response.IndexOf('(');
            int end = response.IndexOf(')');

            if (start == -1 || end == -1)
            {
                throw new InvalidOperationException("Response is not correct, it lacks parantheses.");
            }

            string numbersPart = response.Substring(start + 1, end - start - 1);
            string[] parts = numbersPart.Split(',');

            if (parts.Length != 6)
            {
                throw new InvalidOperationException("Response contains wrong amount of numbers.");
            }

            // 5 and 6 num make teh port num high & low
            int portHigh = int.Parse(parts[4]);
            int portLow = int.Parse(parts[5]);

            return portHigh * 256 + portLow;
        }
    }

    internal class InMemoryDatabaseStorage : IBackendStorage
        {
            public Task StoreFileAsync(string filePath, byte[] data)
            {
                return Task.CompletedTask;
            }

            public Task<byte[]> RetrieveFileAsync(string filePath)
            {
                return Task.FromResult(Encoding.ASCII.GetBytes("Hello, this is a test file."));
            }

            public Task<bool> DeleteFileAsync(string filePath)
            {
                return Task.FromResult(true);
            }

            public Task<IEnumerable<FileItem>> ListAllFilesAsync(string directoryPath)
            {
                return Task.FromResult(Enumerable.Empty<FileItem>());
            }
        }

    public class TestLogger : ILogger<IFtpServer>, ILogger<IAsyncFtpCommandProcessor>
    {
        public List<string> LoggedCommands { get; } = new List<string>();

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            if (message.StartsWith("Received command: "))
            {
                LoggedCommands.Add(message);
            }
        }

        // implementations because of Ilogger
        IDisposable ILogger.BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
    }
}