// <copyright file="FileSystemFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IFileSystemFeature"/>.
    /// </summary>
    internal class FileSystemFeature : IFileSystemFeature, IDisposable
    {
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
        public void Dispose()
        {
            (FileSystem as IDisposable)?.Dispose();
        }
    }
}
