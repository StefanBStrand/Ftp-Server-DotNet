using Group14.FtpServer.CommandHandlers;

namespace Group14.FtpServer.Factories
{
    /// <summary>
    /// Defines a factory for creating FTP command handlers.
    /// </summary>
    public interface IFtpCommandHandlerFactory
    {
        /// <summary>
        /// Creates a command handler for the specified FTP command.
        /// </summary>
        /// <param name="commandName">The name of the FTP command.</param>
        /// <returns>An instance of a command handler that can process the command.</returns>
        /// <exception cref="NotSupportedException">Thrown if the command is not implemented.</exception>
        IFtpCommandHandler CreateHandler(string commandName);
    }
}