namespace Group4.FtpServer
{
    /// <summary>
    /// Defines a contract for formatting file items.
    /// </summary>
    public interface IListFormatter
    {
        /// <summary>
        /// Formats a file item into a string.
        /// </summary>
        /// <param name="item">The file item to format.</param>
        /// <returns>The formatted string.</returns>
        public string FormatFileItem(FileItem item);
    }
}
