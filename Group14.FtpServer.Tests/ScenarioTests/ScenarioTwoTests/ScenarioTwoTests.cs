using System.Net.Sockets;
using System.Text;
using Group14.FtpServer.Handlers;
using Microsoft.Extensions.Logging;

namespace Group14.FtpServer.Tests.ScenarioTests.ScenarioTwoTests
{
    [TestClass]
    public class ScenarioTwoTests
    {
        private FtpServer _server;
        private TestLogger _logger;

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
        public async Task TestScenarioTwoWithoutTls()
        {
            // connection
            using var client = new TcpClient("localhost", 2121);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            // check welcome message
            string response = await reader.ReadLineAsync();
            Assert.AreEqual("220 Welcome to Group-4 FTP server.", response);

            // authenticate user
            await writer.WriteLineAsync("USER test");
            response = await reader.ReadLineAsync();
            Assert.AreEqual("331 Password required", response);

            await writer.WriteLineAsync("PASS 1234");
            response = await reader.ReadLineAsync();
            Assert.AreEqual("230 User logged in.", response);

            // save a file in "db"
            await writer.WriteLineAsync("PASV"); // go to passive mode
            response = await reader.ReadLineAsync();
            int dataPort = GetDataPort(response);

            await writer.WriteLineAsync("STOR testfile.txt"); // start transfering file with STOR
            response = await reader.ReadLineAsync();
            Assert.AreEqual("150 Ready to receive data.", response);

            // connect to dataport and send the file contents
            using (var dataClient = new TcpClient("localhost", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII) { AutoFlush = true })
            {
                await dataWriter.WriteAsync("Hello, this is a test file.");
            }

            response = await reader.ReadLineAsync();
            Assert.AreEqual("226 File stored successfully.", response);

            // get data from db
            await writer.WriteLineAsync("PASV");
            response = await reader.ReadLineAsync();
            dataPort = GetDataPort(response);

            await writer.WriteLineAsync("RETR testfile.txt");
            response = await reader.ReadLineAsync();
            Assert.AreEqual("150 Opening data connection for file transfer.", response);

            using (var retrDataClient = new TcpClient("localhost", dataPort))
            using (var retrDataStream = retrDataClient.GetStream())
            using (var retrReader = new StreamReader(retrDataStream, Encoding.ASCII))
            {
                string content = await retrReader.ReadToEndAsync();
                Assert.AreEqual("Hello, this is a test file.", content);
            }

            response = await reader.ReadLineAsync();
            Assert.AreEqual("226 Transfer complete.", response);

            // get list of files
            await writer.WriteLineAsync("PASV");
            response = await reader.ReadLineAsync();
            dataPort = GetDataPort(response);

            await writer.WriteLineAsync("LIST");
            response = await reader.ReadLineAsync();
            Assert.AreEqual("150 Here is the directory listing", response);

            using (var listDataClient = new TcpClient("localhost", dataPort))
            using (var listDataStream = listDataClient.GetStream())
            using (var listReader = new StreamReader(listDataStream, Encoding.ASCII))
            {
                string listing = await listReader.ReadToEndAsync();
                Assert.IsTrue(listing.Contains("testfile.txt"), "Files has not been found.");
            }

            response = await reader.ReadLineAsync();
            Assert.AreEqual("226 Directory sending ok", response);

            // delete file
            await writer.WriteLineAsync("DELE testfile.txt");
            response = await reader.ReadLineAsync();
            Assert.AreEqual("250 File deleted successfully.", response);

            // verify the file has been deleted
            await writer.WriteLineAsync("PASV");
            response = await reader.ReadLineAsync();
            dataPort = GetDataPort(response);

            await writer.WriteLineAsync("LIST");
            response = await reader.ReadLineAsync();
            Assert.AreEqual("150 Here is the directory listing", response);

            using (var listDataClient = new TcpClient("localhost", dataPort))
            using (var listDataStream = listDataClient.GetStream())
            using (var listReader = new StreamReader(listDataStream, Encoding.ASCII))
            {
                string listing = await listReader.ReadToEndAsync();
                Assert.IsFalse(listing.Contains("testfile.txt"), "Files still exists, even after deletion.");
            }

            response = await reader.ReadLineAsync();
            Assert.AreEqual("226 Directory sending ok", response);

            // verify the logger is logging
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: USER test"), "USER command has not been logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: PASS 1234"), "PASScommand has not been logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: PASV"), "PASV command has not been logged.t");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: STOR testfile.txt"), "STOR command has not been logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: RETR testfile.txt"), "RETR command has not been logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: LIST"), "LIST command has not been logged.");
            Assert.IsTrue(_logger.LoggedCommands.Contains("Received command: DELE testfile.txt"), "DELE command has not been logged.");
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
        private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, FileItem> _fileItems = new Dictionary<string, FileItem>();

        public Task StoreFileAsync(string filePath, byte[] data)
        {
            _files[filePath] = data;
            _fileItems[filePath] = new FileItem
            {
                Name = Path.GetFileName(filePath),
                IsDirectory = false,
                Size = data.Length,
                LastModified = DateTime.Now
            };
            return Task.CompletedTask;
        }

        public Task<byte[]> RetrieveFileAsync(string filePath)
        {
            if (_files.TryGetValue(filePath, out byte[] data))
            {
                return Task.FromResult(data);
            }
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            if (_files.Remove(filePath))
            {
                _fileItems.Remove(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<IEnumerable<FileItem>> ListAllFilesAsync(string directoryPath)
        {
            var items = new List<FileItem>();
            string prefix = directoryPath == "/" ? "/" : directoryPath + "/";

            foreach (var file in _fileItems)
            {
                if (file.Key.StartsWith(prefix) && !file.Key.Substring(prefix.Length).Contains('/'))
                {
                    items.Add(file.Value);
                }
            }
            return Task.FromResult<IEnumerable<FileItem>>(items);
        }
    }

    public class TestLogger : ILogger<IFtpServer>, ILogger<IAsyncFtpCommandProcessor>
    {
        public List<string> LoggedCommands { get; } = new List<string>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            if (message.StartsWith("Received command: "))
            {
                LoggedCommands.Add(message);
            }
        }

        // implementations because of Ilogger
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
    }
}