using System.Runtime.Serialization;

namespace Group14.FtpServer.Exceptions
{
    /// <summary>
    /// Represents an exception where the server is unable to start.
    /// </summary>
    [Serializable]
    internal class FtpServerException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public FtpServerException() { }

        /// <summary>
        /// Constructor taking a message in.
        /// </summary>
        /// <param name="message">Message to display when exception is triggered.</param>
        public FtpServerException(string message)
            : base(message) { }

        /// <summary>
        /// Constructor taking a message and an Exception object.
        /// </summary>
        /// <param name="message">Message to display when exception is triggered.</param>
        /// <param name="inner">The exception that caused this exception.</param>
        public FtpServerException(string message, Exception inner)
            : base(message, inner) { }

        protected FtpServerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
