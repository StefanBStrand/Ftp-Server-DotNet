using System.Runtime.Serialization;

namespace Group14.FtpServer.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs during an FTP command processing.
    /// </summary>
    [Serializable]
    internal class FtpCommandException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public FtpCommandException() { }

        /// <summary>
        /// Constructor taking a message in.
        /// </summary>
        /// <param name="message">Message to display when exception is triggered.</param>
        public FtpCommandException(string message)
            : base(message) { }

        /// <summary>
        /// Constructor taking a message and an Exception object.
        /// </summary>
        /// <param name="message">Message to display when exception is triggered.</param>
        /// <param name="inner">The exception that caused this exception.</param>
        public FtpCommandException(string message, Exception inner)
            : base(message, inner) { }

        protected FtpCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
