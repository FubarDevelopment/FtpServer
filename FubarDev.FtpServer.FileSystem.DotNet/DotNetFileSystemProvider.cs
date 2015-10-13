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

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path for all users</param>
        public DotNetFileSystemProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystem> Create(string userId)
        {
            return Task.FromResult<IUnixFileSystem>(new DotNetFileSystem(Path.Combine(_rootPath, userId)));
        }
    }
}
