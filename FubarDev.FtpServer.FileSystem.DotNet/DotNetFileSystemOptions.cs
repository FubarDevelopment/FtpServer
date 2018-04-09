using System.IO;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    public class DotNetFileSystemOptions
    {
        /// <summary>
        /// Gets or sets the root path for all users
        /// </summary>
        public string RootPath { get; set; } = Path.GetTempPath();
        
        /// <summary>
        /// Gets or sets a value indicating whether the user ID should be used as sub directory.
        /// </summary>
        public bool UseUserIdAsSubFolder { get; set; }
        
        /// <summary>
        /// Gets or sets the buffer size to be used in async IO methods
        /// </summary>
        public int? StreamBufferSize { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether deletion of non-empty directories is allowed.
        /// </summary>
        public bool AllowNonEmptyDirectoryDelete { get; set; }
    }
}
