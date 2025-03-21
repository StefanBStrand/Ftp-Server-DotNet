namespace Group4.FtpServer
{
    /// <summary>
    /// Formats file items in UNIX-style for the FTP directory listings.
    /// </summary>
    public class UnixListFormatter : IListFormatter
    {
        /// <summary>
        /// Formats a file item into a UNIX-style string.
        /// </summary>
        /// <param name="item">The item to format.</param>
        /// <returns>Formatted string representing the file item.</returns>
        public string FormatFileItem(FileItem item)
        {
            // set type based on if its a dir or file
            string type = item.IsDirectory ? "drwxr-xr-x" : "-rw-r--r--";

            // format date and size
            string size = item.Size.ToString().PadLeft(8);
            string date = item.LastModified.ToString("MMM dd HH:mm");

            string line = type + " 1 user group ";
            line = line + size + " ";
            line = line + date + " ";
            line = line + item.Name;

            return line;
        }
    }
}
