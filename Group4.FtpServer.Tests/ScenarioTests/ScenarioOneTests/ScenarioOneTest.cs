// Scenario One Test 
// Start FTP server with minimal config, none or optional TLS
// Simulate a clinet connecting to the serverr
// AUthentication with correct credentials.m 
// Commands: PWD, CWD, LIST, QUIT.. more?

using System.Text;
using System.Net.Sockets;

namespace Group4.FtpServer.Tests.ScenarioTests.ScenarioOneTests
{
    [TestClass]
    public class ScenarioOneTest
    {
        private IFtpServer _ftpServer = null!;

        [TestInitialize]
        public async Task Setup()
        {
            var options = new FtpServerOptions()
            {
                EnableTls = false,
                Port = 2122,
                RootPath = "./TestFtpRoot"
            };

            _ftpServer = new FtpServer(options);
            await _ftpServer.StartAsync();
        }


        [TestCleanup]
        public async Task Teardown()
        {
            await _ftpServer.StopAsync();

            if (Directory.Exists("./TestFtpRoot"))
                Directory.Delete("./TestFtpRoot", true);
        }

        [TestMethod]
        public async Task ScenarioOne_BasicWorkflowWithoutTls_Success()
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", 2122);

                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true })
                {

                    string welcome = (await reader.ReadLineAsync())!;
                    Assert.IsTrue(welcome.StartsWith("220"));

                    await writer.WriteLineAsync("USER test");
                    string userResponse = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("331 Password required", userResponse);

                    await writer.WriteLineAsync("PASS 1234");
                    string passResponse = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("230 User logged in.", passResponse);

                    await writer.WriteLineAsync("PWD");
                    string pwdResponse = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("257 \"/\" is the current directory", pwdResponse);

                    await writer.WriteLineAsync("CWD /testdir");
                    string cwdResponse = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("250 Directory changed successfully.", cwdResponse);

                    await writer.WriteLineAsync("PWD");
                    pwdResponse = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("257 \"/testdir\" is the current directory", pwdResponse);

                    await writer.WriteLineAsync("PASV");
                    string pasvResponse = (await reader.ReadLineAsync())!;
                    Assert.IsTrue(pasvResponse.StartsWith("227"));

                    var parts = pasvResponse.Split('(')[1].Split(')')[0].Split(',');
                    string pasvIp = $"{parts[0]}.{parts[1]}.{parts[2]}.{parts[3]}";
                    int pasvPort = int.Parse(parts[4]) * 256 + int.Parse(parts[5]);

                    await writer.WriteLineAsync("LIST");
                    string listPreliminary = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("150 Here is the directory listing", listPreliminary);

                    using (TcpClient dataClient = new TcpClient())
                    {
                        await dataClient.ConnectAsync(pasvIp, pasvPort);
                        using (StreamReader dataReader = new StreamReader(dataClient.GetStream(), Encoding.ASCII))
                        {
                            string listData = await dataReader.ReadToEndAsync();
                            Assert.IsNotNull(listData);
                        }
                    }

                    string listComplete = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("226 Directory sending ok", listComplete);

                    await writer.WriteLineAsync("QUIT");
                    string quitResponse = (await reader.ReadLineAsync())!;
                    Assert.AreEqual("221 Goodbye", quitResponse);

                }
            }
        }

    }
}