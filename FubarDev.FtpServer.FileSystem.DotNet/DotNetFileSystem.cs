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
        public DotNetFileSystem(string rootPath)
        {
            FileSystemEntryComparer = StringComparer.OrdinalIgnoreCase;
            Root = new DotNetDirectoryEntry(Directory.CreateDirectory(rootPath));
        }

        /// <inheritdoc/>
        public StringComparer FileSystemEntryComparer { get; }

        /// <inheritdoc/>
        public IUnixDirectoryEntry Root { get; }

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
                    result.Add(new DotNetDirectoryEntry(dirInfo));
                }
                else
                {
                    var fileInfo = info as FileInfo;
                    if (fileInfo != null)
                    {
                        result.Add(new DotNetFileEntry(fileInfo));
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
                result = new DotNetFileEntry(new FileInfo(fullPath));
            else if (Directory.Exists(fullPath))
                result = new DotNetDirectoryEntry(new DirectoryInfo(fullPath));
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
                return Task.FromResult<IUnixFileSystemEntry>(new DotNetFileEntry(new FileInfo(targetName)));
            }

            var sourceDirEntry = (DotNetDirectoryEntry)source;
            sourceDirEntry.Info.MoveTo(targetName);
            return Task.FromResult<IUnixFileSystemEntry>(new DotNetDirectoryEntry(new DirectoryInfo(targetName)));
        }

        /// <inheritdoc/>
        public async Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var dirEntry = entry as DotNetDirectoryEntry;
            if (dirEntry != null)
            {
                dirEntry.Info.Delete();
            }
            else
            {
                var fileEntry = (DotNetFileEntry)entry;
                fileEntry.Info.Delete();
            }
            await Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            var targetEntry = (DotNetDirectoryEntry)targetDirectory;
            var newDirInfo = targetEntry.Info.CreateSubdirectory(directoryName);
            return Task.FromResult<IUnixDirectoryEntry>(new DotNetDirectoryEntry(newDirInfo));
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
