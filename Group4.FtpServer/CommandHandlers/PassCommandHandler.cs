namespace Group4.FtpServer.CommandHandlers
{
    /// <summary>
    /// Handles the pass command for user authentication
    /// </summary>
    public class PassCommandHandler : IAsyncFtpCommandHandler
    {

        private readonly IAuthenticationProvider _authenticationProvider;
        private const string AlreadyAuthenticatedResponse = "503 Already logged in.";
        private const string NoUsernameResponse = "503 Login with USER first.";
        private const string SyntaxErrorResponse = "501 Syntax error in parameters.";
        private const string AuthenticationSuccessResponse = "230 User logged in.";
        private const string AuthenticationFailureResponse = "530 Authentication failed.";

        /// <summary>
        /// Gets the command string this handler processes.
        /// </summary>
        public string Command => "PASS";

        /// <summary>
        /// Initializes a new instance of the PassCommandHandler class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider responsible for verifying the user.</param>
        /// <exception cref="ArgumentNullException">Thrown if authenticationProvider is null.</exception>
        public PassCommandHandler(IAuthenticationProvider authenticationProvider)
        {
            if (authenticationProvider == null)
                throw new ArgumentNullException(nameof(authenticationProvider), "Authentication provider can't be null.");

            _authenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Processes the PASS command to authenticate the user with the provided password.
        /// </summary>
        /// <param name="command">The full command string received from the client.</param>
        /// <param name="connection">The active connection to the client.</param>
        /// <param name="session">The current FTP session state.</param>
        /// <returns>A response string indicating the result of the authentication attempt.</returns>

        public Task<string> HandleCommandAsync(string command, IAsyncFtpConnection connection, IFtpSession session)
        {
            if (session.IsAuthenticated)
            {
                return Task.FromResult(AlreadyAuthenticatedResponse);
            }

            if (string.IsNullOrEmpty(session.Username))
            {
                return Task.FromResult(NoUsernameResponse);
            }

            var commandArguments = command.Split(' ', 2);
            if (commandArguments.Length < 2)
            {
                return Task.FromResult(SyntaxErrorResponse);
            }

            var password = commandArguments[1].Trim();
            var isAuthenticated = _authenticationProvider.Authenticate(session.Username, password);
            if (isAuthenticated)
            {
                session.IsAuthenticated = true;
                return Task.FromResult(AuthenticationSuccessResponse);
            }

            return Task.FromResult(AuthenticationFailureResponse);
        }
    }
}
