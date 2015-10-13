//-----------------------------------------------------------------------
// <copyright file="DotNetFileSystemProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    /// <summary>
    /// A <see cref="IFileSystemClassFactory"/> implementation that uses
    /// the standard .NET functionality to provide file system access.
    /// </summary>
    public class DotNetFileSystemProvider : IFileSystemClassFactory
    {
        private readonly string _rootPath;

        private readonly bool _useUserIdAsSubFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path for all users</param>
        public DotNetFileSystemProvider(string rootPath)
            : this(rootPath, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path for all users</param>
        /// <param name="useUserIdAsSubFolder">Use the user id as subfolder?</param>
        public DotNetFileSystemProvider(string rootPath, bool useUserIdAsSubFolder)
        {
            _rootPath = rootPath;
            _useUserIdAsSubFolder = useUserIdAsSubFolder;
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystem> Create(string userId)
        {
            var path = _rootPath;
            if (_useUserIdAsSubFolder)
                path = Path.Combine(path, userId);
            return Task.FromResult<IUnixFileSystem>(new DotNetFileSystem(path));
        }
    }
}
