namespace Group14.FtpServer.CommandHandlers
{
    public class PbszCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "PBSZ";

        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return Task.FromResult("530 Please login with USER and PASS.");

            return Task.FromResult("200 PBSZ command successful.");
        }
    }
}
