using System.Net.Sockets;
using System.Text;
using Group4.FtpServer.Handlers;

namespace Group4.FtpServer.Tests.ScenarioTests.ScenarioThreeTests
{
    [TestClass]
    public class ScenarioThree_CloudBasedFtpTests
    {
        private IFtpServer _ftpServer;

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
            // connect
            using var client = new TcpClient("127.0.0.1", 2124);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            // confirm wlecome message
            string welcome = await reader.ReadLineAsync();
            Assert.IsTrue(welcome.StartsWith("220"), "Server should have started with 220 Welcome.");

            // tests if prompts for password after user command
            await writer.WriteLineAsync("USER test");
            string userResp = await reader.ReadLineAsync();
            Assert.AreEqual("331 Password required", userResp);

            // test that authentication works correctly
            await writer.WriteLineAsync("PASS 1234");
            string passResp = await reader.ReadLineAsync();
            Assert.AreEqual("230 User logged in.", passResp);

            // enter passive mode and extract data port from servers response
            // (necessary to do before file transfer commands to know wher to connect for data)
            await writer.WriteLineAsync("PASV");
            string pasvResp = await reader.ReadLineAsync();
            int dataPort = ExtractPortFromPasvResponse(pasvResp);

            // verify server is ready to store files
            await writer.WriteLineAsync("STOR mycloudfile.txt");
            string storResp = await reader.ReadLineAsync();
            Assert.AreEqual("150 Ready to receive data.", storResp);

            // simulate uploading a file
            using (var dataClient = new TcpClient("127.0.0.1", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII) { AutoFlush = true })
            {
                await dataWriter.WriteAsync("Hello Cloud!");
            }
            string storComplete = await reader.ReadLineAsync();
            Assert.AreEqual("226 File stored successfully.", storComplete);

            // request pasv again for file retrieval
            await writer.WriteLineAsync("PASV");
            pasvResp = await reader.ReadLineAsync();
            dataPort = ExtractPortFromPasvResponse(pasvResp);

            // verify retrieving response is correct
            await writer.WriteLineAsync("RETR mycloudfile.txt");
            string retrResp = await reader.ReadLineAsync();
            Assert.AreEqual("150 Opening data connection for file transfer.", retrResp);

            // read file content
            using (var retrClient = new TcpClient("127.0.0.1", dataPort))
            using (var retrStream = retrClient.GetStream())
            using (var retrReader = new StreamReader(retrStream, Encoding.ASCII))
            {
                string content = await retrReader.ReadToEndAsync();
                Assert.AreEqual("Hello Cloud!", content);
            }

            // verify retr complete
            string retrComplete = await reader.ReadLineAsync();
            Assert.AreEqual("226 Transfer complete.", retrComplete);

            // pasv mode again for dir listing
            await writer.WriteLineAsync("PASV");
            pasvResp = await reader.ReadLineAsync();
            dataPort = ExtractPortFromPasvResponse(pasvResp);

            // check first resposne after LIST is being sent
            await writer.WriteLineAsync("LIST");
            string listPrelim = await reader.ReadLineAsync();
            Assert.AreEqual("150 Here is the directory listing", listPrelim);

            // verify the listing inludes mycloudfile.txt
            using (var listClient = new TcpClient("127.0.0.1", dataPort))
            using (var listStream = listClient.GetStream())
            using (var listReader = new StreamReader(listStream, Encoding.ASCII))
            {
                string listing = await listReader.ReadToEndAsync();
                Assert.IsTrue(listing.Contains("mycloudfile.txt"), "List should contain mycloudfile.txt.");
            }

            // verify server finishes and responds correctly
            string listDone = await reader.ReadLineAsync();
            Assert.AreEqual("226 Directory sending ok", listDone);

            // confirm the server replies with goodbye after QUIT
            await writer.WriteLineAsync("QUIT");
            string quitResp = await reader.ReadLineAsync();
            Assert.AreEqual("221 Goodbye", quitResp);
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
            private readonly Dictionary<string, byte[]> _cloudStorage = new Dictionary<string, byte[]>();
            private readonly Dictionary<string, FileItem> _cloudFileItems = new Dictionary<string, FileItem>();

            public Task StoreFileAsync(string filePath, byte[] data)
            {
                _cloudStorage[filePath] = data;
                _cloudFileItems[filePath] = new FileItem
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
                if (!_cloudStorage.ContainsKey(filePath))
                {
                    throw new FileNotFoundException($"Can't find file in cloud: {filePath}");
                }

                return Task.FromResult(_cloudStorage[filePath]);
            }

            public Task<bool> DeleteFileAsync(string filePath)
            {
                if (_cloudStorage.Remove(filePath))
                {
                    _cloudFileItems.Remove(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }

            public Task<IEnumerable<FileItem>> ListAllFilesAsync(string directoryPath)
            {
                var items = new List<FileItem>();

                string prefix = directoryPath == "/" ? "/" : directoryPath + "/";

                foreach (var entry in _cloudFileItems)
                {
                    if (!entry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string remainder = entry.Key.Substring(prefix.Length);

                    if (!remainder.Contains("/"))
                    {
                        items.Add(entry.Value);
                    }
                }

                return Task.FromResult<IEnumerable<FileItem>>(items);
            }
        }
    }

}
