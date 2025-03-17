namespace Group14.FtpServer
{
    /// <summary>
    /// Represents a local file storage backend.
    /// </summary>
    public class LocalFileStorage : IBackendStorage
    {

        private readonly string _rootPath;

        /// <summary>
        /// Initializes a new instance of the LocalFileStorage class with a specified root directory.
        /// </summary>
        /// <param name="rootPath">The root directory for storing files. If null or empty, the current directory is used.</param>
        public LocalFileStorage(string rootPath)
        {
            // set root to current dir if empty
            if (string.IsNullOrEmpty(rootPath))
            {
                _rootPath = Directory.GetCurrentDirectory();
            }
            else
            {
                _rootPath = Path.GetFullPath(rootPath);
            }

            // make dir if doesnt exist
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
        }

        /// <summary>
        /// Initializes a new instance of the LocalFileStorage class using the current directory as the root.
        /// </summary>
        public LocalFileStorage()
            : this(null) { }

        /// <summary>
        /// Stores a file at the specified path.
        /// </summary>
        /// <param name="filePath">The path where the file should be stored, included the file name</param>
        /// <param name="data">The binary content of the file.</param>
        public void StoreFile(string filePath, byte[] data)
        {
            // build path
            string fullPath = Path.Combine(_rootPath, filePath.TrimStart('/')).Replace('/', Path.DirectorySeparatorChar);
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(fullPath, data);
        }

        /// <summary>
        /// Retrieves the content of a file.
        /// </summary>
        /// <param name="filePath">The path to the file to retrieve, included the file name</param>
        /// <returns>The content of a file in binary.</returns>
        public byte[] RetrieveFile(string filePath)
        {
            string fullPath = Path.Combine(_rootPath, filePath.TrimStart('/'));
            fullPath = fullPath.Replace('/', Path.DirectorySeparatorChar);

            // read file and return
            byte[] data = File.ReadAllBytes(fullPath);
            return data; 
            
            // TODO EXCEPTION!
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="filePath">The path to the file to delete, included the file name</param>
        /// <returns>True or false based if the file successfully is deleted or not.</returns>
        public bool DeleteFile(string filePath)
        {
            string fullPath = Path.Combine(_rootPath, filePath.TrimStart('/'));
            fullPath = fullPath.Replace('/', Path.DirectorySeparatorChar);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lists all files in a specific directory.
        /// </summary>
        /// <param name="ftpPath">The path to the directory.</param>
        /// <returns>a list of file items in the directory</returns>
        public IEnumerable<FileItem> ListAllFiles(string ftpPath)
        {
            string localPath = Path.Combine(_rootPath, ftpPath.TrimStart('/'));
            var items = new List<FileItem>();

            if (!Directory.Exists(localPath))
            {
                return items;
            }

            foreach (var dir in Directory.GetDirectories(localPath))
            {
                var info = new DirectoryInfo(dir);
                items.Add(new FileItem
                {
                    Name = Path.GetFileName(dir),
                    IsDirectory = true,
                    Size = 0,
                    LastModified = info.LastWriteTime
                });
            }

            foreach (var file in Directory.GetFiles(localPath))
            {
                var info = new FileInfo(file);
                items.Add(new FileItem
                {
                    Name = Path.GetFileName(file),
                    IsDirectory = false,
                    Size = info.Length,
                    LastModified = info.LastWriteTime
                });
            }
            return items;
        }
    }
}