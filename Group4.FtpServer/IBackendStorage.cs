namespace Group4.FtpServer
{
    /// <summary>
    /// Defines a contract for a backend storage system
    /// </summary>
    public interface IBackendStorage
    {
        /// <summary>
        /// Stores a file at the specified path.
        /// </summary>
        /// <param name="filePath">The path where the file should be stored, included the file name</param>
        /// <param name="data">The binary content of the file.</param>
        Task StoreFileAsync(string filePath, byte[] data);

        /// <summary>
        /// Retrieves the content of a file.
        /// </summary>
        /// <param name="filePath">The path to the file to retrieve, included the file name</param>
        /// <returns>The content of a file in binary.</returns>
        Task<byte[]> RetrieveFileAsync(string filePath);

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="filePath">The path to the file to delete, included the file name</param>
        /// <returns>True or false based if the file successfully is deleted or not.</returns>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// Lists all files in a specific directory
        /// </summary>
        /// <param name="directoryPath">The path to the directory.</param>
        /// <returns>A list of file items in the directory</returns>
        Task<IEnumerable<FileItem>> ListAllFilesAsync(string directoryPath);
    }
}
