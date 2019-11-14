// <copyright file="FileSystemFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IFileSystemFeature"/>.
    /// </summary>
    internal class FileSystemFeature : IFileSystemFeature, IResettableFeature
    {
        private Stack<IUnixDirectoryEntry> _initialPath = new Stack<IUnixDirectoryEntry>();

        /// <inheritdoc />
        public IUnixFileSystem FileSystem { get; set; } = new EmptyUnixFileSystem();

        /// <inheritdoc />
        public Stack<IUnixDirectoryEntry> Path { get; set; } = new Stack<IUnixDirectoryEntry>();

        /// <inheritdoc />
        public IUnixDirectoryEntry CurrentDirectory
        {
            get
            {
                if (Path.Count == 0)
                {
                    return FileSystem.Root;
                }

                return Path.Peek();
            }
        }

        /// <inheritdoc />
        public void SetInitialPath(Stack<IUnixDirectoryEntry> path)
        {
            _initialPath = path.Clone();
            Path = _initialPath.Clone();
        }

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken)
        {
            (FileSystem as IDisposable)?.Dispose();
            FileSystem = new EmptyUnixFileSystem();
            SetInitialPath(new Stack<IUnixDirectoryEntry>());
            return Task.CompletedTask;
        }
    }
}
