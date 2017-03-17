//-----------------------------------------------------------------------
// <copyright file="DotNetFileSystemProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

using JetBrains.Annotations;

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

        private readonly int _streamBufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path for all users</param>
        public DotNetFileSystemProvider([NotNull] string rootPath)
            : this(rootPath, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path for all users</param>
        /// <param name="useUserIdAsSubFolder">Use the user id as subfolder?</param>
        public DotNetFileSystemProvider([NotNull] string rootPath, bool useUserIdAsSubFolder)
            : this(rootPath, useUserIdAsSubFolder, DotNetFileSystem.DEFAULT_STREAM_BUFFER_SIZE)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path for all users</param>
        /// <param name="useUserIdAsSubFolder">Use the user id as subfolder?</param>
        /// <param name="streamBufferSize">Buffer size to be used in async IO methods</param>
        public DotNetFileSystemProvider([NotNull] string rootPath, bool useUserIdAsSubFolder, int streamBufferSize)
        {
            _rootPath = rootPath;
            _useUserIdAsSubFolder = useUserIdAsSubFolder;
            _streamBufferSize = streamBufferSize;
        }

        /// <summary>
        /// Gets or sets a value indicating whether deletion of non-empty directories is allowed.
        /// </summary>
        public bool AllowNonEmptyDirectoryDelete { get; set; }

        /// <inheritdoc/>
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var path = _rootPath;
            if (_useUserIdAsSubFolder)
            {
                if (isAnonymous)
                    userId = "anonymous";
                path = Path.Combine(path, userId);
            }

            return Task.FromResult<IUnixFileSystem>(new DotNetFileSystem(path, AllowNonEmptyDirectoryDelete, _streamBufferSize));
        }
    }
}
