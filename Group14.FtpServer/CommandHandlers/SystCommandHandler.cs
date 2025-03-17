namespace Group14.FtpServer.CommandHandlers
{
    internal class SystCommandHandler : IAsyncFtpCommandHandler
    {
        public string Command => "SYST";

        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
                return Task.FromResult("530 Please login with USER and PASS.");

            return Task.FromResult("215 UNIX Type: L8"); 
        }
    }
}
