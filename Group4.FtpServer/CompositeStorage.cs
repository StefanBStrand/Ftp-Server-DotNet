namespace Group4.FtpServer
{
    /// <summary>
    /// A composite storage backend that routes file operations to different backends based on directory paths.
    /// </summary>
    public class CompositeStorage : IBackendStorage
    {
        private readonly IBackendStorage _defaultBackend;
        private readonly Dictionary<string, IBackendStorage> _backendMappings;

        /// <summary>
        /// Initializes a new instance of the CompositeStorage class.
        /// </summary>
        /// <param name="defaultBackend">The default backend to use when no specific mapping is found.</param>
        /// <param name="backendMappings">A dictionary mapping directory paths to specific backends.</param>
        /// <exception cref="ArgumentNullException">Thrown if defaultBackend is null.</exception>
        public CompositeStorage(IBackendStorage defaultBackend, Dictionary<string, IBackendStorage> backendMappings)
        {
            if (defaultBackend == null)
                throw new ArgumentNullException(nameof(defaultBackend), "Default backend can't be null.");
            
            _defaultBackend = defaultBackend;

            _backendMappings = backendMappings ?? new Dictionary<string, IBackendStorage>();
        }

        /// <summary>
        /// Determines the appropriate backend for a given file path using longest prefix matching.
        /// </summary>
        /// <param name="path">The file or directory path.</param>
        /// <returns>The backend responsible for the path, or the default backend if no match is found.</returns>
        private IBackendStorage GetBackendForPath(string path)
        {
            string normalizedPath = NormalizePath(path);
            string? bestMatch = null; // start with null as the best match
            int bestMatchLength = -1;

            foreach (var mapping in _backendMappings) // go through each mapping 
            {
                string mappingPath = NormalizePath(mapping.Key); // normalize "key" 

                // check if the normalized path starts with curr mapping and if its longer than current best match
                if (normalizedPath.StartsWith(mappingPath) && mappingPath.Length > bestMatchLength)
                {
                    // if so, then update 
                    bestMatch = mappingPath;
                    bestMatchLength = mappingPath.Length;
                }
            }


            if (bestMatch != null)
            {
                return _backendMappings[bestMatch];
            }
            else
            {
                return _defaultBackend;
            }
        }

        /// <summary>
        /// Normalizes a path to ensure consistency (uses forward slashes, trims extra slashes).
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        private string NormalizePath(string path)
        {
            /// Why? FTP clients can send messy paths like \local\file.txt or //local//file.txt ...
            /// We normalize to /local/file.txt so it matches our beautiful mapping for /local.
            if (string.IsNullOrEmpty(path))
                return "/";
            return "/" + path.Trim('/').Replace('\\', '/');
        }

        /// <summary>
        /// Stores a file at the specified path using the appropriate backend.
        /// </summary>
        public async Task StoreFileAsync(string filePath, byte[] data)
        {
            var backend = GetBackendForPath(filePath);
            await backend.StoreFileAsync(filePath, data);
        }

        /// <summary>
        /// Retrieves a file from the specified path using the appropriate backend.
        /// </summary>
        public async Task<byte[]> RetrieveFileAsync(string filePath)
        {
            var backend = GetBackendForPath(filePath);
            return await backend.RetrieveFileAsync(filePath);
        }

        /// <summary>
        /// Deletes a file at the specified path using the appropriate backend.
        /// </summary>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            var backend = GetBackendForPath(filePath);
            return await backend.DeleteFileAsync(filePath);
        }

        /// <summary>
        /// Lists all files in the specified directory using the appropriate backend.
        /// </summary>
        public async Task<IEnumerable<FileItem>> ListAllFilesAsync(string directoryPath)
        {
            var backend = GetBackendForPath(directoryPath);
            return await backend.ListAllFilesAsync(directoryPath);
        }
    }
}