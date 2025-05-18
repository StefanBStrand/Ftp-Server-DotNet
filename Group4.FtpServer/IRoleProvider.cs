
namespace Group4.FtpServer
{
    /// <summary>
    /// Defines an interface for providing user roles.
    /// </summary>
    public interface IRoleProvider
    {
        /// <summary>
        /// Gets the role for the specified username
        /// </summary>
        /// <param name="username">The username to retrieve the role for.</param>
        /// <returns>The role of the ser, or null if the user does not exist.</returns>
        string? GetRole(string username);
    }
}
