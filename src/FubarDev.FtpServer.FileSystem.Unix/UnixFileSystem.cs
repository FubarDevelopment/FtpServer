// <copyright file="UnixFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.BackgroundTransfer;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal class UnixFileSystem : IUnixFileSystem
    {
        private readonly IFtpUser _user;

        public UnixFileSystem(
            IUnixDirectoryEntry root,
            IFtpUser user)
        {
            _user = user;
            Root = root;
        }

        /// <inheritdoc />
        public bool SupportsAppend { get; } = true;

        /// <inheritdoc />
        public bool SupportsNonEmptyDirectoryDelete { get; } = false;

        /// <inheritdoc />
        public StringComparer FileSystemEntryComparer { get; } = StringComparer.Ordinal;

        /// <inheritdoc />
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc />
        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> MoveAsync(
            IUnixDirectoryEntry parent,
            IUnixFileSystemEntry source,
            IUnixDirectoryEntry target,
            string fileName,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(
            IUnixDirectoryEntry targetDirectory,
            string directoryName,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IBackgroundTransfer> CreateAsync(
            IUnixDirectoryEntry targetDirectory,
            string fileName,
            Stream data,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> SetMacTimeAsync(
            IUnixFileSystemEntry entry,
            DateTimeOffset? modify,
            DateTimeOffset? access,
            DateTimeOffset? create,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
