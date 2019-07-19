// <copyright file="DirectoryListing.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Utilities
{
    /// <summary>
    /// Helps to enumerate a directory with virtual <c>.</c> and <c>..</c> entries.
    /// </summary>
    public class DirectoryListing : IAsyncEnumerable<DirectoryListingEntry>
    {
        private readonly Stack<IUnixDirectoryEntry> _pathEntries;
        private readonly IUnixFileSystem _fileSystem;
        private readonly IUnixDirectoryEntry _currentDirectory;
        private readonly IUnixDirectoryEntry? _parentDirectory;
        private readonly IUnixDirectoryEntry? _grandParentDirectory;
        private readonly IAsyncEnumerable<IUnixFileSystemEntry> _entries;
        private readonly List<(IUnixDirectoryEntry entry, string name)> _dotEntries = new List<(IUnixDirectoryEntry entry, string name)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryListing"/> class.
        /// </summary>
        /// <param name="entries">The file system entries to enumerate.</param>
        /// <param name="fileSystem">The file system of the file system entries.</param>
        /// <param name="pathEntries">The path entries of the current directory.</param>
        /// <param name="returnDotEntries"><code>true</code> when this enumerator should return the dot entries.</param>
        public DirectoryListing(
            IAsyncEnumerable<IUnixFileSystemEntry> entries,
            IUnixFileSystem fileSystem,
            Stack<IUnixDirectoryEntry> pathEntries,
            bool returnDotEntries)
        {
            _entries = entries;
            _fileSystem = fileSystem;
            _pathEntries = pathEntries;

            var topPathEntries = pathEntries.Take(3).ToList();

            switch (topPathEntries.Count)
            {
                case 0:
                    _currentDirectory = fileSystem.Root;
                    _parentDirectory = null;
                    _grandParentDirectory = null;
                    break;
                case 1:
                    _currentDirectory = topPathEntries[0];
                    _parentDirectory = fileSystem.Root;
                    _grandParentDirectory = null;
                    break;
                case 2:
                    _currentDirectory = topPathEntries[0];
                    _parentDirectory = topPathEntries[1];
                    _grandParentDirectory = fileSystem.Root;
                    break;
                default:
                    _currentDirectory = topPathEntries[0];
                    _parentDirectory = topPathEntries[1];
                    _grandParentDirectory = topPathEntries[2];
                    break;
            }

            if (returnDotEntries)
            {
                _dotEntries.Add((CurrentDirectory, "."));
                if (ParentDirectory != null)
                {
                    _dotEntries.Add((ParentDirectory, ".."));
                }
            }
        }

        /// <summary>
        /// Gets the file system of the entries to be enumerated.
        /// </summary>
        public IUnixFileSystem FileSystem => _fileSystem;

        /// <summary>
        /// Gets the current directory.
        /// </summary>
        public IUnixDirectoryEntry CurrentDirectory => _currentDirectory;

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        public IUnixDirectoryEntry? ParentDirectory => _parentDirectory;

        /// <summary>
        /// Gets the grand parent directory.
        /// </summary>
        public IUnixDirectoryEntry? GrandParentDirectory => _grandParentDirectory;

        /// <inheritdoc />
        public IAsyncEnumerator<DirectoryListingEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new DirectoryListingEntryEnumerator(
                _pathEntries,
                _fileSystem,
                _currentDirectory,
                _parentDirectory,
                _grandParentDirectory,
                _entries.GetAsyncEnumerator(cancellationToken),
                _dotEntries.GetEnumerator());
        }

        private class DirectoryListingEntryEnumerator : IAsyncEnumerator<DirectoryListingEntry>
        {
            private readonly Stack<IUnixDirectoryEntry> _pathEntries;
            private readonly IUnixFileSystem _fileSystem;
            private readonly IUnixDirectoryEntry _currentDirectory;
            private readonly IUnixDirectoryEntry? _parentDirectory;
            private readonly IUnixDirectoryEntry? _grandParentDirectory;
            private readonly IAsyncEnumerator<IUnixFileSystemEntry> _entriesEnumerator;
            private readonly IEnumerator<(IUnixDirectoryEntry entry, string name)> _dotEntriesEnumerator;
            private bool _enumerateDotEntries = true;
            private DirectoryListingEntry? _current;

            public DirectoryListingEntryEnumerator(
                Stack<IUnixDirectoryEntry> pathEntries,
                IUnixFileSystem fileSystem,
                IUnixDirectoryEntry currentDirectory,
                IUnixDirectoryEntry? parentDirectory,
                IUnixDirectoryEntry? grandParentDirectory,
                IAsyncEnumerator<IUnixFileSystemEntry> entriesEnumerator,
                IEnumerator<(IUnixDirectoryEntry entry, string name)> dotEntriesEnumerator)
            {
                _pathEntries = pathEntries;
                _fileSystem = fileSystem;
                _currentDirectory = currentDirectory;
                _parentDirectory = parentDirectory;
                _grandParentDirectory = grandParentDirectory;
                _entriesEnumerator = entriesEnumerator;
                _dotEntriesEnumerator = dotEntriesEnumerator;
            }

            /// <inheritdoc />
            public DirectoryListingEntry Current => _current ?? throw new InvalidOperationException("Current was called before MoveNextAsync");

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                _dotEntriesEnumerator.Dispose();
                return _entriesEnumerator.DisposeAsync();
            }

            /// <inheritdoc />
            public async ValueTask<bool> MoveNextAsync()
            {
                if (_enumerateDotEntries)
                {
                    if (_dotEntriesEnumerator.MoveNext())
                    {
                        var (entry, name) = _dotEntriesEnumerator.Current;
                        _current = new DirectoryListingEntry(
                            _pathEntries,
                            _fileSystem,
                            _currentDirectory,
                            _parentDirectory,
                            _grandParentDirectory,
                            entry,
                            name);
                        return true;
                    }

                    _enumerateDotEntries = false;
                }

                if (await _entriesEnumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    var entry = _entriesEnumerator.Current;
                    _current = new DirectoryListingEntry(
                        _pathEntries,
                        _fileSystem,
                        _currentDirectory,
                        _parentDirectory,
                        _grandParentDirectory,
                        entry);
                    return true;
                }

                return false;
            }
        }
    }
}
