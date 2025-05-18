using System.Net.Sockets;
using System.Text;
using Group4.FtpServer.Handlers;

namespace Group4.FtpServer.Tests.ScenarioTests.ScenarioThreeTests
{
    [TestClass]
    public class ScenarioThreeTests
    {
        private IFtpServer _ftpServer = null!;

        [TestInitialize]
        public async Task Setup()
        {
            var options = new FtpServerOptions
            {
                Port = 2124,
                EnableTls = false,    
                RootPath = null
            };

            var cloudStorage = new CloudFileStorageMock();
            var authProvider = new SimpleAuthenticationProvider(); 
            var listFormatter = new UnixListFormatter();

            var commandProcessor = new FtpCommandProcessor(cloudStorage, authProvider, listFormatter, options);
            var listener = new TcpConnectionListener(options);
            var sessionFactory = new DefaultFtpSessionFactory(cloudStorage);

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
        public async Task Scenario3_BasicWorkflowWithoutTls_Success()
        {
            using var client = new TcpClient("127.0.0.1", 2124);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            string welcome = (await reader.ReadLineAsync())!;
            Assert.IsTrue(welcome.StartsWith("220"), "Server should have started with 220 Welcome.");
            await writer.WriteLineAsync("USER test");
            Assert.AreEqual("331 Password required", await reader.ReadLineAsync());
            await writer.WriteLineAsync("PASS 1234");
            Assert.AreEqual("230 User logged in.", await reader.ReadLineAsync());

            await writer.WriteLineAsync("PASV");
            string pasvResp = (await reader.ReadLineAsync())!;
            int dataPort = ExtractPortFromPasvResponse(pasvResp);
            await writer.WriteLineAsync("STOR mycloudfile.txt");
            Assert.AreEqual("150 Ready to receive data.", await reader.ReadLineAsync());
            using (var dataClient = new TcpClient("127.0.0.1", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII) { AutoFlush = true })
            {
                await dataWriter.WriteAsync("Hello Cloud!");
            }
            Assert.AreEqual("226 File stored successfully.", await reader.ReadLineAsync());

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

        internal class CloudFileStorageMock : IBackendStorage
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
