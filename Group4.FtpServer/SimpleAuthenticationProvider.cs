namespace Group4.FtpServer
{
    /// <summary>
    /// Simple authentication provider.
    /// </summary>
    public class SimpleAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SimpleAuthenticationProvider() { }

        /// <summary>
        /// Authenticates a user with hardcoded credentials
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>True if authentication succeeds, false otherwise.</returns>
        public bool Authenticate(string username, string password)
        {
            return username == "test" && password == "1234";
        }
    }
}
