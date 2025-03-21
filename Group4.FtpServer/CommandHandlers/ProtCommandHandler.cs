namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the PROT command to set the data protection level for secure transfers.
    /// </summary>
    public class ProtCommandHandler : IAsyncFtpCommandHandler
    {
        private const string NotAuthenticatedResponse = "530 Please login with USER and PASS.";
        private const string InvalidParameterResponse = "504 Command not implemented for that parameter.";
        private const string SuccessResponse = "200 PROT command successful.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "PROT";


        /// <summary>
        /// Processes the PROT command to set the data protection level.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the result of the operation.</returns>
        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (!session.IsAuthenticated)
            {
                return Task.FromResult(NotAuthenticatedResponse);
            }

            var commandArguments = command.Split(' ');
            if (commandArguments.Length < 2 || (commandArguments[1] != "C" && commandArguments[1] != "P"))
            {
                return Task.FromResult(InvalidParameterResponse);
            }

            return Task.FromResult(SuccessResponse);
        }
    }
}
