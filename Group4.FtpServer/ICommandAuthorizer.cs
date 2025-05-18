namespace Group4.FtpServer
{
    /// <summary>
    /// Defines an interface for authorized the FTP commands based on user roles.
    /// </summary>
    public interface ICommandAuthorizer
    {
        /// <summary>
        /// Determines whether the specified command given is authorized for given session.
        /// </summary>
        /// <param name="session">The FTP session containing user information</param>
        /// <param name="command">The FTP command to authorize.</param>
        /// <returns>True if command is authorized, otherwise false.</returns>
        bool isAuthorized(IFtpSession session, string command);
    }
}
