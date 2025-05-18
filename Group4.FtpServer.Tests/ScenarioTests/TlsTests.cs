namespace Group4.FtpServer.Tests.ScenarioTests
{
    using Group4.FtpServer;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using System.IO;
    using System.Net.Security;
    using System.Text;

    [TestClass]
    public class TlsTests
    {
        private FtpServer _ftpServer = null!;

        private X509Certificate2 GenerateSelfSignedCertificate() 
            // we generate a self-signed certificate instead of using openssl 
            // note this is juts for testing purposes
        {
            using (var rsa = System.Security.Cryptography.RSA.Create(2048)) // RSA key pair with a key size of 2048 bits
            {
                var request = new CertificateRequest(  // create a cert request for our server
                    "CN=TestFtpServer",
                    rsa,
                    System.Security.Cryptography.HashAlgorithmName.SHA256,
                    System.Security.Cryptography.RSASignaturePadding.Pkcs1);

                // create a self-signed certificate which is going to be valid from yesterday untill one year from now on
                var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

                // EXPORT and reimport the certifiacte to ensure it is able to be exported
                return new X509Certificate2(
                    certificate.Export(X509ContentType.Pfx, "password"),
                    "password",
                    X509KeyStorageFlags.Exportable);
            }
        }

        [TestInitialize]
        public async Task Setup()
        {
            var options = new FtpServerOptions()
            {
                EnableTls = true,
                Port = 2123,      
                RootPath = "./TestFtpRoot",
                Certificate = GenerateSelfSignedCertificate() // add our certificate
            };

            _ftpServer = new FtpServer(options);
            await _ftpServer.StartAsync();

            Directory.CreateDirectory(options.RootPath);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _ftpServer.StopAsync();
            if (Directory.Exists("./TestFtpRoot"))
            {
                Directory.Delete("./TestFtpRoot", true);
            }
        }

        [TestMethod]
        public async Task Scenarios_TlsUpgrade_Success()
        {
            using (var client = new TcpClient("127.0.0.1", 2123))
            {
                // Now! Obtain the underlying NETWORK stream for initial plain text coms
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.ASCII);
                var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                var welcomeMessage = await reader.ReadLineAsync();
                Assert.AreEqual("220 Welcome to Group-4 FTP server.", welcomeMessage);

                // send AUTH TLS command to request an upgrade to the secured connection (ssl)
                await writer.WriteLineAsync("AUTH TLS");
                var response = await reader.ReadLineAsync();
                Assert.AreEqual("234 Proceed with negotiation.", response);

                /*
                 * We actually have now a working TLS. Why didn't it work before? After hours after hours of figuring why
                 * And why the Frame was corrupted is:
                 * We originally had something like this:
                 * 
                 * using (SslStream sslStream = new SslStream(netStream, leaveInnerStreamOpen: false, ...))
                 * 
                 * What happens if we leaveInnerStreamOpen to false?
                 * It means if the SslStream is going to get disposed, we are also closing the udnerlying NetworkStream...
                 * BUT!! -> FTP with TLS demands that you DONT close teh underlying stream when TLS ends, because
                 * 1. SslStream can buffer data (like quit for example) and send it out AFTER it closes
                 * 2. If you close networkstream simutaneously you also cut off the data BEFORE they have the time to go out
                 * 3. Result -> Server gets a broken TLS package -> Frame corruption which we struggled for hours with.
                 */
                using (var sslStream = new SslStream(stream, false, (sender, cert, chain, errors) => true))
                    // we're upgrading the connection to TLS by wrapping the existing stream (stream (NetworkStream)) in an SslStream
                {
                    // initiate the handshake with the server usign "localhost" for now -> as the target host
                    await sslStream.AuthenticateAsClientAsync("localhost");

                    // verify tls handshake -> celebrate
                    Assert.IsTrue(sslStream.IsEncrypted);
                    Assert.IsTrue(sslStream.IsAuthenticated);

                    // now create new reader and writer objects for coms over the encrypted stream
                    var sslWriter = new StreamWriter(sslStream, Encoding.ASCII) { AutoFlush = true };
                    var sslReader = new StreamReader(sslStream, Encoding.ASCII);

                    await sslWriter.WriteLineAsync("QUIT"); // send quit over TLS
                    var quitResponse = await sslReader.ReadLineAsync();

                    // verify response -> double celebrate
                    Assert.IsNotNull(quitResponse);
                }
            }
        }
    }
}
