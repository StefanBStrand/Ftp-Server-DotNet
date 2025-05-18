namespace Group4.FtpServer
{
    /// <summary>
    /// Implementation of command authorization based on user roles.
    /// </summary>
    public class RoleBasedCommandAuthorizer : ICommandAuthorizer
    {

        private readonly IRoleProvider _roleProvider;
        private readonly Dictionary<string, List<string>> _commandPermissions;

        /// <summary>
        /// Initializes a new instance of RoleBasedCommandAuthorizer class.
        /// </summary>
        /// <param name="roleProvider">The provider for user roles.</param>
        /// <param name="commandPermissions">A dictionary mapping commands to lists of allwoed roles.</param>
        /// <exception cref="ArgumentNullException">Thrown if either parameters are null.</exception>
        public RoleBasedCommandAuthorizer(IRoleProvider roleProvider, Dictionary<string, List<string>> commandPermissions)
        {
            if (roleProvider == null)
                throw new ArgumentNullException(nameof(roleProvider), "Role provider can't be null.");
            if (commandPermissions == null)
                throw new ArgumentNullException(nameof(commandPermissions), "Command permissions list can't be null.");

            _roleProvider = roleProvider;
            _commandPermissions = commandPermissions;
        }

        /// <summary>
        /// Determines whether the specified command is authorized for given session.
        /// </summary>
        /// <param name="session">The FTP session containting user information.</param>
        /// <param name="command">The FTP command to authorize.</param>
        /// <returns>True if the command is authorized, otherwise null.</returns>
        public bool isAuthorized(IFtpSession session, string command)
        {
            string commandUpper = command.ToUpper();

            // we always allows these 3
            if (commandUpper == "USER" || commandUpper == "PASS" || commandUpper == "QUIT")
                return true;

            if (session == null || string.IsNullOrEmpty(session.Username) || !session.IsAuthenticated)
                return false;

            var role = _roleProvider.GetRole(session.Username);
            if (string.IsNullOrEmpty(role))
                return false;

            if (_commandPermissions.TryGetValue(commandUpper, out var allowedRoles))
                return allowedRoles.Contains(role);

            return true;
        }

    }
}
