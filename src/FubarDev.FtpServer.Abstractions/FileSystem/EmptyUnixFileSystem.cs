// <copyright file="EmptyUnixFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// An empty file system to be used when the user isn't logged in yet.
    /// </summary>
    public class EmptyUnixFileSystem : IUnixFileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyUnixFileSystem"/> class.
        /// </summary>
        public EmptyUnixFileSystem()
        {
            Root = new EmptyRootDirectory(this);
        }

        /// <inheritdoc/>
        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc/>
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc/>
        public bool SupportsAppend => false;

        /// <inheritdoc/>
        public bool SupportsNonEmptyDirectoryDelete => false;

        /// <inheritdoc/>
        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(new List<IUnixFileSystemEntry>());
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            return Task.FromResult<IUnixFileSystemEntry>(null);
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private class EmptyRootDirectory : IUnixDirectoryEntry
        {
            public EmptyRootDirectory(EmptyUnixFileSystem fileSystem)
            {
                FileSystem = fileSystem;
                var accessMode = new GenericAccessMode(true, false, false);
                Permissions = new GenericUnixPermissions(accessMode, accessMode, accessMode);
            }

            public DateTimeOffset? CreatedTime => null;

            public IUnixFileSystem FileSystem { get; }

            public string Group => "group";

            public bool IsDeletable => false;

            public bool IsRoot => true;

            public DateTimeOffset? LastWriteTime => null;

            public string Name => string.Empty;

            public long NumberOfLinks => 1;

            public string Owner => "owner";

            public IUnixPermissions Permissions { get; }
        }
    }
}
