// <copyright file="InMemoryFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;

namespace FubarDev.FtpServer.FileSystem.InMemory
{
    /// <summary>
    /// The implementation of the in-memory file system.
    /// </summary>
    public class InMemoryFileSystem : IUnixFileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystem"/> class.
        /// </summary>
        /// <param name="fileSystemEntryComparer">The file system entry name comparer.</param>
        public InMemoryFileSystem(StringComparer fileSystemEntryComparer)
        {
            Root = new InMemoryDirectoryEntry(
                this,
                null,
                string.Empty,
                new Dictionary<string, IUnixFileSystemEntry>(fileSystemEntryComparer));
            FileSystemEntryComparer = fileSystemEntryComparer;
        }

        /// <inheritdoc />
        public bool SupportsAppend { get; } = true;

        /// <inheritdoc />
        public bool SupportsNonEmptyDirectoryDelete { get; } = true;

        /// <inheritdoc />
        public StringComparer FileSystemEntryComparer { get; }

        /// <inheritdoc />
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc />
        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(
            IUnixDirectoryEntry directoryEntry,
            CancellationToken cancellationToken)
        {
            var entry = (InMemoryDirectoryEntry)directoryEntry;
            var children = entry.Children.Values.ToList();
            return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(children);
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var entry = (InMemoryDirectoryEntry)directoryEntry;
            if (entry.Children.TryGetValue(name, out var childEntry))
            {
                return Task.FromResult(childEntry);
            }

            return Task.FromResult<IUnixFileSystemEntry>(null);
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> MoveAsync(
            IUnixDirectoryEntry parent,
            IUnixFileSystemEntry source,
            IUnixDirectoryEntry target,
            string fileName,
            CancellationToken cancellationToken)
        {
            var parentEntry = (InMemoryDirectoryEntry)parent;
            var sourceEntry = (InMemoryFileSystemEntry)source;
            var targetEntry = (InMemoryDirectoryEntry)target;

            targetEntry.Children.Add(fileName, source);

            if (!parentEntry.Children.Remove(source.Name))
            {
                targetEntry.Children.Remove(fileName);
                return Task.FromResult<IUnixFileSystemEntry>(null);
            }

            var now = DateTimeOffset.Now;
            parentEntry.SetLastWriteTime(now);
            targetEntry.SetLastWriteTime(now);

            sourceEntry.Parent = targetEntry;

            return Task.FromResult(source);
        }

        /// <inheritdoc />
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var fsEntry = (InMemoryFileSystemEntry)entry;
            if (fsEntry.Parent?.Children.Remove(entry.Name) ?? false)
            {
                fsEntry.Parent.SetLastWriteTime(DateTimeOffset.Now);
                fsEntry.Parent = null;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(
            IUnixDirectoryEntry targetDirectory,
            string directoryName,
            CancellationToken cancellationToken)
        {
            var dirEntry = (InMemoryDirectoryEntry)targetDirectory;
            var childEntry = new InMemoryDirectoryEntry(
                this,
                dirEntry,
                directoryName,
                new Dictionary<string, IUnixFileSystemEntry>(FileSystemEntryComparer));
            dirEntry.Children.Add(directoryName, childEntry);
            var now = DateTimeOffset.Now;
            dirEntry.SetLastWriteTime(now)
                .SetCreateTime(now);
            return Task.FromResult<IUnixDirectoryEntry>(childEntry);
        }

        /// <inheritdoc />
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var entry = (InMemoryFileEntry)fileEntry;
            var stream = new MemoryStream(entry.Data)
            {
                Position = startPosition,
            };

            return Task.FromResult<Stream>(stream);
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            var entry = (InMemoryFileEntry)fileEntry;

            // Copy original data into memory stream
            var temp = new MemoryStream();
            temp.Write(entry.Data, 0, entry.Data.Length);

            // Set new write position (if given)
            if (startPosition is long startPos)
            {
                temp.Position = startPos;
            }

            // Copy given data
            await data.CopyToAsync(temp, 81920, cancellationToken)
                .ConfigureAwait(false);

            // Update data
            entry.Data = temp.ToArray();
            entry.SetLastWriteTime(DateTimeOffset.Now);

            return null;
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer> CreateAsync(
            IUnixDirectoryEntry targetDirectory,
            string fileName,
            Stream data,
            CancellationToken cancellationToken)
        {
            var temp = new MemoryStream();
            await data.CopyToAsync(temp, 81920, cancellationToken)
                .ConfigureAwait(false);

            var targetEntry = (InMemoryDirectoryEntry)targetDirectory;
            var entry = new InMemoryFileEntry(this, targetEntry, fileName, temp.ToArray());
            targetEntry.Children.Add(fileName, entry);

            var now = DateTimeOffset.Now;
            targetEntry.SetLastWriteTime(now);

            entry
                .SetLastWriteTime(now)
                .SetCreateTime(now);

            return null;
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var temp = new MemoryStream();
            await data.CopyToAsync(temp, 81920, cancellationToken)
                .ConfigureAwait(false);

            var entry = (InMemoryFileEntry)fileEntry;
            entry.Data = temp.ToArray();

            var now = DateTimeOffset.Now;
            entry.SetLastWriteTime(now);

            return null;
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> SetMacTimeAsync(
            IUnixFileSystemEntry entry,
            DateTimeOffset? modify,
            DateTimeOffset? access,
            DateTimeOffset? create,
            CancellationToken cancellationToken)
        {
            var fsEntry = (InMemoryFileSystemEntry)entry;

            if (modify != null)
            {
                fsEntry.SetLastWriteTime(modify.Value);
            }

            if (create != null)
            {
                fsEntry.SetCreateTime(create.Value);
            }

            return Task.FromResult(entry);
        }
    }
}
