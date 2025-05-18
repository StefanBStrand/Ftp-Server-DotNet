using System.Net.Sockets;
using System.Text;
using Group4.FtpServer.Handlers;

namespace Group4.FtpServer.Tests.ScenarioTests.ScenarioFourTests
{
    [TestClass]
    public class ScenarioFourTests
    {
        private IFtpServer _ftpServer = null!;

        [TestInitialize]
        public async Task Setup()
        {
            var options = new FtpServerOptions
            {
                Port = 2125,
                EnableTls = false,
                RootPath = null // not using with composite storage
            };

            var localBackend = new MockLocalFileStorage();
            var cloudBackend = new MockCloudFileStorage();

            var backendMappings = new Dictionary<string, IBackendStorage>
            {
                { "/local", localBackend },
                { "/cloud", cloudBackend }
            };

            var compositeStorage = new CompositeStorage(localBackend, backendMappings);

            var authProvider = new SimpleAuthenticationProvider();
            var listFormatter = new UnixListFormatter();

            var commandProcessor = new FtpCommandProcessor(compositeStorage, authProvider, listFormatter, options);
            var listener = new TcpConnectionListener(options);
            var sessionFactory = new DefaultFtpSessionFactory(compositeStorage);

            _ftpServer = new FtpServer(listener, commandProcessor, sessionFactory);
            await _ftpServer.StartAsync();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (_ftpServer != null)
            {
                await _ftpServer.StopAsync();
            }
        }

        [TestMethod]
        public async Task Scenario4_BasicWorkflowWithoutTls_Success()
        {
            using var client = new TcpClient("127.0.0.1", 2125);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            string welcome = (await reader.ReadLineAsync())!;
            Assert.IsTrue(welcome.StartsWith("220"), "Server should respond with 220 Welcome.");
            await writer.WriteLineAsync("USER test");
            Assert.AreEqual("331 Password required", (await reader.ReadLineAsync())!);
            await writer.WriteLineAsync("PASS 1234");
            Assert.AreEqual("230 User logged in.", (await reader.ReadLineAsync())!);

            // store a file in /local
            await writer.WriteLineAsync("PASV");
            string pasvResp = (await reader.ReadLineAsync())!;
            int dataPort = ExtractPortFromPasvResponse(pasvResp);
            await writer.WriteLineAsync("STOR /local/myfile.txt");
            Assert.AreEqual("150 Ready to receive data.", (await reader.ReadLineAsync())!);

            using (var dataClient = new TcpClient("127.0.0.1", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII) { AutoFlush = true })
            {
                await dataWriter.WriteAsync("Hello Local!");
            }
            Assert.AreEqual("226 File stored successfully.", await reader.ReadLineAsync());

            // retrieve file from /local
            await writer.WriteLineAsync("PASV");
            pasvResp = (await reader.ReadLineAsync())!;
            dataPort = ExtractPortFromPasvResponse(pasvResp);
            await writer.WriteLineAsync("RETR /local/myfile.txt");
            Assert.AreEqual("150 Opening data connection for file transfer.", await reader.ReadLineAsync());

            using (var dataClient = new TcpClient("127.0.0.1", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataReader = new StreamReader(dataStream, Encoding.ASCII))
            {
                string content = await dataReader.ReadToEndAsync();
                Assert.AreEqual("Hello Local!", content);
            }
            Assert.AreEqual("226 Transfer complete.", await reader.ReadLineAsync());

            // store a file in /cloud 
            await writer.WriteLineAsync("PASV");
            pasvResp = (await reader.ReadLineAsync())!;
            dataPort = ExtractPortFromPasvResponse(pasvResp);
            await writer.WriteLineAsync("STOR /cloud/mycloudfile.txt");
            Assert.AreEqual("150 Ready to receive data.", await reader.ReadLineAsync());

            using (var dataClient = new TcpClient("127.0.0.1", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII) { AutoFlush = true })
            {
                await dataWriter.WriteAsync("Hello Cloud!");
            }
            Assert.AreEqual("226 File stored successfully.", await reader.ReadLineAsync());

            // retrive file from /cloud
            await writer.WriteLineAsync("PASV");
            pasvResp = (await reader.ReadLineAsync())!;
            dataPort = ExtractPortFromPasvResponse(pasvResp);
            await writer.WriteLineAsync("RETR /cloud/mycloudfile.txt");
            Assert.AreEqual("150 Opening data connection for file transfer.", await reader.ReadLineAsync());

            using (var dataClient = new TcpClient("127.0.0.1", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataReader = new StreamReader(dataStream, Encoding.ASCII))
            {
                string content = await dataReader.ReadToEndAsync();
                Assert.AreEqual("Hello Cloud!", content);
            }
            Assert.AreEqual("226 Transfer complete.", await reader.ReadLineAsync());

            await writer.WriteLineAsync("QUIT");
            Assert.AreEqual("221 Goodbye", await reader.ReadLineAsync());
        }

        private int ExtractPortFromPasvResponse(string pasvResponse)
        {
            int start = pasvResponse.IndexOf('(');
            int end = pasvResponse.IndexOf(')');
            string numbersPart = pasvResponse.Substring(start + 1, end - start - 1);
            var parts = numbersPart.Split(',');

            int high = int.Parse(parts[4]);
            int low = int.Parse(parts[5]);
            return high * 256 + low;
        }

        internal class MockLocalFileStorage : IBackendStorage
        {
            public Task StoreFileAsync(string filePath, byte[] data)
            {
                return Task.CompletedTask;
            }

            public Task<byte[]> RetrieveFileAsync(string filePath)
            {
                return Task.FromResult(Encoding.ASCII.GetBytes("Hello Local!"));
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

        internal class MockCloudFileStorage : IBackendStorage
        {
            public Task StoreFileAsync(string filePath, byte[] data)
            {
                return Task.CompletedTask;
            }

            public Task<byte[]> RetrieveFileAsync(string filePath)
            {
                return Task.FromResult(Encoding.ASCII.GetBytes("Hello Cloud!"));
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
    }
}