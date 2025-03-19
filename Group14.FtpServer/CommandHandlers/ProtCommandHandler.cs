namespace Group14.FtpServer.CommandHandlers
{
    public class ProtCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "PROT";

        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return Task.FromResult("530 Please login with USER and PASS.");

            var parts = command.Split(' ');

            if (parts.Length < 2 || (parts[1] != "C" && parts[1] != "P"))
                return Task.FromResult("504 Command not implemented for that parameter.");

            return Task.FromResult("200 PROT command successful.");
        }
    }
}
