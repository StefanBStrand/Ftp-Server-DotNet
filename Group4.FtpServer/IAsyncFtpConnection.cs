namespace Group4.FtpServer
{
    /// <summary>
    /// Defines a contract for an asynchronous FTP connection to a client.
    /// </summary>
    public interface IAsyncFtpConnection : IDisposable
    {
        /// <summary>
        /// Gets the stream used for data transfer with the client.
        /// </summary>
        /// <returns>The underlying stream for the connection.</returns>
        public Stream GetStream();

        /// <summary>
        /// Reads a command sent by the client asynchronously.
        /// </summary>
        /// <returns>A task that resolves to the command string or null if the connection is closed.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if reading fails due to a network error.</exception>
        Task<string> ReadCommandAsync();

        /// <summary>
        /// Sends a response to the client asynchronously.
        /// </summary>
        /// <param name="response">The response string to send.</param>
        /// <exception cref="ArgumentException">Thrown if response is null or empty.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if sending fails due to a network error.</exception>
        Task SendResponseAsync(string response);

        /// <summary>
        /// Closes the connection asynchronously, releasing all associated resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        Task CloseAsync();
    }
}
