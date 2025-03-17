namespace Group14.FtpServer.CommandHandlers
{
    internal class FeatCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "FEAT";

        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            return Task.FromResult(
                "211-Features:\r\n" +
                " AUTH TLS\r\n" +
                " PBSZ\r\n" +
                " PROT\r\n" +
                " UTF8\r\n" +
                "211 End"
            );
        }
    }
}
