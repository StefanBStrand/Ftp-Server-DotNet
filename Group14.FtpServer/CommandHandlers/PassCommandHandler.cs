namespace Group14.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the pass command for user authentication
    /// </summary>
    internal class PassCommandHandler : IFtpCommandHandler
    {

        private readonly IAuthenticationProvider _authenticationProvider;

        /// <summary>
        /// Initializes a new instance of the PassCommandHandler class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider responsible for verifying the user.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if "authenticationProvider is null.
        /// </exception>
        public PassCommandHandler(IAuthenticationProvider authenticationProvider)
        {
            if (authenticationProvider == null)
                throw new ArgumentNullException(nameof(authenticationProvider), "Authentication provider can't be null.");

            _authenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Processes an FTP command and returns a response
        /// </summary>
        /// <param name="command">The full command string from the client</param>
        /// <param name="connection">The connection to the client</param>
        /// <param name="session">The current session state</param>
        /// <returns>FTP response code and message</returns>
        public string HandleCommand(string command, IFtpConnection connection, IFtpSession session)
        {
            if (session.IsAuthenticated)
                return "503 Already logged in.";

            if (string.IsNullOrEmpty(session.Username))
                return "503 Login with USER first.";

            var parts = command.Split(' ', 2);
            if (parts.Length < 2)
                return "501 Syntax error in parameters.";

            string password = parts[1].Trim();
            bool authenticated = _authenticationProvider.Authenticate(session.Username, password);

            if (_authenticationProvider.Authenticate(session.Username, password))
            {
                session.IsAuthenticated = true;
                return "230 User logged in.";
            }

            return "530 Authentication failed.";
        }
    }
}
