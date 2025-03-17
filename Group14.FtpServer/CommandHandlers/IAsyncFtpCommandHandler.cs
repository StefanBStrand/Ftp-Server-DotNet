namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Defines a contract for an FTP command handler.
    /// </summary>
    public interface IAsyncFtpCommandHandler
    {
        public string Command { get; }

        Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session);
    }
}
