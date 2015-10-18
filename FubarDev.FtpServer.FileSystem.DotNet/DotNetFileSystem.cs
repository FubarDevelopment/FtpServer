//-----------------------------------------------------------------------
// <copyright file="DotNetFileSystem.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    /// <summary>
    /// A <see cref="IUnixFileSystem"/> implementation that uses the
    /// standard .NET functionality to access the file system.
    /// </summary>
    public class DotNetFileSystem : IUnixFileSystem
    {
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystem"/> class.
        /// </summary>
        /// <param name="rootPath">The path to use as root</param>
        /// <param name="allowNonEmptyDirectoryDelete">Allow deletion of non-empty directories?</param>
        public DotNetFileSystem(string rootPath, bool allowNonEmptyDirectoryDelete)
        {
            FileSystemEntryComparer = StringComparer.OrdinalIgnoreCase;
            Root = new DotNetDirectoryEntry(this, Directory.CreateDirectory(rootPath), true);
            SupportsNonEmptyDirectoryDelete = allowNonEmptyDirectoryDelete;
        }

        /// <inheritdoc/>
        public bool SupportsNonEmptyDirectoryDelete { get; }

        /// <inheritdoc/>
        public StringComparer FileSystemEntryComparer { get; }

        /// <inheritdoc/>
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc/>
        public bool SupportsAppend => true;

        /// <inheritdoc/>
        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var result = new List<IUnixFileSystemEntry>();
            var searchDirInfo = ((DotNetDirectoryEntry)directoryEntry).Info;
            foreach (var info in searchDirInfo.EnumerateFileSystemInfos())
            {
                var dirInfo = info as DirectoryInfo;
                if (dirInfo != null)
                {
                    result.Add(new DotNetDirectoryEntry(this, dirInfo, false));
                }
                else
                {
                    var fileInfo = info as FileInfo;
                    if (fileInfo != null)
                    {
                        result.Add(new DotNetFileEntry(this, fileInfo));
                    }
                }
            }
            return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(result);
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var searchDirInfo = ((DotNetDirectoryEntry)directoryEntry).Info;
            var fullPath = Path.Combine(searchDirInfo.FullName, name);
            IUnixFileSystemEntry result;
            if (File.Exists(fullPath))
                result = new DotNetFileEntry(this, new FileInfo(fullPath));
            else if (Directory.Exists(fullPath))
                result = new DotNetDirectoryEntry(this, new DirectoryInfo(fullPath), false);
            else
                result = null;
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            var targetEntry = (DotNetDirectoryEntry)target;
            var targetName = Path.Combine(targetEntry.Info.FullName, fileName);

            var sourceFileEntry = source as DotNetFileEntry;
            if (sourceFileEntry != null)
            {
                sourceFileEntry.Info.MoveTo(targetName);
                return Task.FromResult<IUnixFileSystemEntry>(new DotNetFileEntry(this, new FileInfo(targetName)));
            }

            var sourceDirEntry = (DotNetDirectoryEntry)source;
            sourceDirEntry.Info.MoveTo(targetName);
            return Task.FromResult<IUnixFileSystemEntry>(new DotNetDirectoryEntry(this, new DirectoryInfo(targetName), false));
        }

        /// <inheritdoc/>
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var dirEntry = entry as DotNetDirectoryEntry;
            if (dirEntry != null)
            {
                dirEntry.Info.Delete(SupportsNonEmptyDirectoryDelete);
            }
            else
            {
                var fileEntry = (DotNetFileEntry)entry;
                fileEntry.Info.Delete();
            }
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            var targetEntry = (DotNetDirectoryEntry)targetDirectory;
            var newDirInfo = targetEntry.Info.CreateSubdirectory(directoryName);
            return Task.FromResult<IUnixDirectoryEntry>(new DotNetDirectoryEntry(this, newDirInfo, false));
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var fileInfo = ((DotNetFileEntry)fileEntry).Info;
            var input = fileInfo.OpenRead();
            if (startPosition != 0)
                input.Seek(startPosition, SeekOrigin.Begin);
            return Task.FromResult<Stream>(input);
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            var fileInfo = ((DotNetFileEntry)fileEntry).Info;
            using (var output = fileInfo.OpenWrite())
            {
                if (startPosition == null)
                    startPosition = fileInfo.Length;
                output.Seek(startPosition.Value, SeekOrigin.Begin);
                await data.CopyToAsync(output, 4096, cancellationToken);
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            var targetEntry = (DotNetDirectoryEntry)targetDirectory;
            var fileInfo = new FileInfo(Path.Combine(targetEntry.Info.FullName, fileName));
            using (var output = fileInfo.Create())
            {
                await data.CopyToAsync(output, 4096, cancellationToken);
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var fileInfo = ((DotNetFileEntry)fileEntry).Info;
            using (var output = fileInfo.OpenWrite())
            {
                await data.CopyToAsync(output, 4096, cancellationToken);
                output.SetLength(output.Position);
            }
            return null;
        }

        /// <summary>
        /// Sets the modify/access/create timestamp of a file system item
        /// </summary>
        /// <param name="entry">The <see cref="IUnixFileSystemEntry"/> to change the timestamp for</param>
        /// <param name="modify">The modification timestamp</param>
        /// <param name="access">The access timestamp</param>
        /// <param name="create">The creation timestamp</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The modified <see cref="IUnixFileSystemEntry"/></returns>
        public Task<IUnixFileSystemEntry> SetMacTime(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            var dirEntry = entry as DotNetDirectoryEntry;
            var fileEntry = entry as DotNetFileEntry;
            var item = dirEntry == null ? (FileSystemInfo)fileEntry.Info : dirEntry.Info;
            if (access != null)
                item.LastAccessTimeUtc = access.Value.UtcDateTime;
            if (modify != null)
                item.LastWriteTimeUtc = modify.Value.UtcDateTime;
            if (create != null)
                item.CreationTimeUtc = create.Value.UtcDateTime;
            if (dirEntry != null)
                return Task.FromResult<IUnixFileSystemEntry>(new DotNetDirectoryEntry(this, (DirectoryInfo)item, dirEntry.IsRoot));
            return Task.FromResult<IUnixFileSystemEntry>(new DotNetFileEntry(this, (FileInfo)item));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <param name="disposing"><code>true</code> when called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Nothing to dispose
                }
                _disposedValue = true;
            }
        }
    }
}
