namespace Group4.FtpServer
{
    /// <summary>
    /// Represents a file or directory in the ftp server
    /// </summary>
    public class FileItem
    {
        /// <summary>
        /// Gets or sets the name of the file or directory
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a directory
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the file
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
