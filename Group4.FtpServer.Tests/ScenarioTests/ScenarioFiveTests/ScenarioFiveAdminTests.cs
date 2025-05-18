using System.Net.Sockets;
using System.Text;
using Group4.FtpServer.Handlers;
using Group4.FtpServer.Tests.ScenarioTests.ScenarioTwoTests;

namespace Group4.FtpServer.Tests.ScenarioTests.ScenarioFiveTests
{
    [TestClass]
    public class ScenarioFiveAdminTests
    {
        private FtpServer _server = null!;
        private TestLogger _logger = null!;
        private const int Port = 2127;

        [TestInitialize]
        public void Setup()
        {
            var storage = new InMemoryDatabaseStorage();
            var authProvider = new SimpleAuthenticationProvider();

            var roleProvider = new SimpleRoleProvider(new Dictionary<string, string>
            {
                { "test", "admin" }
            });

            var permissions = new Dictionary<string, List<string>>
            {
                { "STOR", new List<string> { "admin" } },
                { "DELE", new List<string> { "admin" } },
                { "RETR", new List<string> { "admin", "user" } },
                { "LIST", new List<string> { "admin", "user" } }
            };

            var authorizer = new RoleBasedCommandAuthorizer(roleProvider, permissions);
            var listFormatter = new UnixListFormatter();
            var options = new FtpServerOptions { Port = Port, EnableTls = false };

            _logger = new TestLogger();

            var commandProcessor = new FtpCommandProcessor(
                storage, authProvider, listFormatter, options, _logger, authorizer);

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
        public async Task Scenario5_AdminWorkflow_AllOperationsAllowed_Success()
        {
            using var client = new TcpClient("localhost", Port);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            // authenticate at the start
            await reader.ReadLineAsync();
            await writer.WriteLineAsync("USER test");
            await reader.ReadLineAsync();
            await writer.WriteLineAsync("PASS 1234");
            await reader.ReadLineAsync(); 

            // upload a file
            await writer.WriteLineAsync("PASV");
            int dataPort = GetDataPort((await reader.ReadLineAsync())!);
            await writer.WriteLineAsync("STOR upload.txt");
            Assert.AreEqual("150 Ready to receive data.", await reader.ReadLineAsync());

            using (var dataClient = new TcpClient("localhost", dataPort))
            using (var dataStream = dataClient.GetStream())
            using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII) { AutoFlush = true })
                await dataWriter.WriteAsync("hello");

            Assert.AreEqual("226 File stored successfully.", await reader.ReadLineAsync());

            // download (retr)
            await writer.WriteLineAsync("PASV");
            dataPort = GetDataPort((await reader.ReadLineAsync())!);
            await writer.WriteLineAsync("RETR somefile.txt");
            Assert.IsTrue((await reader.ReadLineAsync())!.StartsWith("150"));

            using (var dataClient = new TcpClient("localhost", dataPort))
            using (var dataReader = new StreamReader(dataClient.GetStream(), Encoding.ASCII))
                Assert.AreEqual("Hello, this is a test file.", await dataReader.ReadToEndAsync());

            Assert.IsTrue((await reader.ReadLineAsync())!.StartsWith("226"));

            // files them ifles
            await writer.WriteLineAsync("PASV");
            dataPort = GetDataPort((await reader.ReadLineAsync())!);
            await writer.WriteLineAsync("LIST");
            Assert.IsTrue((await reader.ReadLineAsync())!.StartsWith("150"));

            using (var dataClient = new TcpClient("localhost", dataPort))
            using (var dataReader = new StreamReader(dataClient.GetStream(), Encoding.ASCII))
                Assert.AreEqual(string.Empty, await dataReader.ReadToEndAsync());

            Assert.IsTrue((await reader.ReadLineAsync())!.StartsWith("226"));

            // delete something
            await writer.WriteLineAsync("DELE somefile.txt");
            Assert.AreEqual("250 File deleted successfully.", await reader.ReadLineAsync());

            // quit he session
            await writer.WriteLineAsync("QUIT");
            Assert.AreEqual("221 Goodbye", await reader.ReadLineAsync());
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
}
