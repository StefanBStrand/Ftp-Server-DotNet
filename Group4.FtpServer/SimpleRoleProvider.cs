
namespace Group4.FtpServer
{
    /// <summary>
    /// A simple in memory role provider for testing purposes.
    /// </summary>
    public class SimpleRoleProvider : IRoleProvider
    {
        private readonly Dictionary<string, string> _userRoles;

        /// <summary>
        /// Initializes a new instnace of SimpleRoleProvider class.
        /// </summary>
        /// <param name="userRoles">A dictionary mapping usernames to teh roles.</param>
        /// <exception cref="ArgumentNullException">Thrown if dictionary provided is null.</exception>
        public SimpleRoleProvider(Dictionary<string, string> userRoles)
        {
            if (userRoles == null)
                throw new ArgumentNullException(nameof(userRoles), "User roles dictionary can't be null.");

            _userRoles = userRoles;
        }

        /// <summary>
        /// Gets the role for the specified username.
        /// </summary>
        /// <param name="username">The username to retrieve the role for.</param>
        /// <returns>The role of the user or null if the user does not exist.</returns>
        public string? GetRole(string username)
        {
            _userRoles.TryGetValue(username, out string? role);
            
            return role;
        }
    }
}
