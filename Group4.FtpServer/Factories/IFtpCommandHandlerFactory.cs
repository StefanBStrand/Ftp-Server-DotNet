using Group4.FtpServer.CommandHandlers;

namespace Group4.FtpServer.Factories
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
        /// <exception cref="ArgumentException">Thrown if commandName is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if the command is not supported by the server.</exception>
        IAsyncFtpCommandHandler CreateHandler(string commandName);
    }
}