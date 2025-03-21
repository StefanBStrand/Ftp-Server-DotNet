namespace Group4.FtpServer
{
    /// <summary>
    /// Defines a contract for user authentication.
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Authenticates a user with the given username and password.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>True if authentication succeeds, false otherwise.</returns>
        public bool Authenticate(string username, string password);
    }
}
