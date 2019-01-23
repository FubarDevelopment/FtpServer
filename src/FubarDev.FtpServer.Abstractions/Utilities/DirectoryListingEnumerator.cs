// <copyright file="DirectoryListingEnumerator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Utilities
{
    /// <summary>
    /// Helps to enumerate a directory with virtual <c>.</c> and <c>..</c> entries.
    /// </summary>
    public class DirectoryListingEnumerator
    {
        private readonly Stack<IUnixDirectoryEntry> _pathEntries;

        private readonly IEnumerator<IUnixFileSystemEntry> _entriesEnumerator;

        private readonly IEnumerator<Tuple<IUnixDirectoryEntry, string>> _dotEntriesEnumerator;

        private bool _enumerateDotEntries = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryListingEnumerator"/> class.
        /// </summary>
        /// <param name="entries">The file system entries to enumerate.</param>
        /// <param name="fileSystem">The file system of the file system entries.</param>
        /// <param name="pathEntries">The path entries of the current directory.</param>
        /// <param name="returnDotEntries"><code>true</code> when this enumerator should return the dot entries.</param>
        public DirectoryListingEnumerator(IEnumerable<IUnixFileSystemEntry> entries, IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> pathEntries, bool returnDotEntries)
        {
            _pathEntries = pathEntries;

            var topPathEntries = pathEntries.Take(3).ToList();

            switch (topPathEntries.Count)
            {
                case 0:
                    CurrentDirectory = fileSystem.Root;
                    ParentDirectory = GrandParentDirectory = null;
                    break;
                case 1:
                    CurrentDirectory = topPathEntries[0];
                    ParentDirectory = fileSystem.Root;
                    GrandParentDirectory = null;
                    break;
                case 2:
                    CurrentDirectory = topPathEntries[0];
                    ParentDirectory = topPathEntries[1];
                    GrandParentDirectory = fileSystem.Root;
                    break;
                default:
                    CurrentDirectory = topPathEntries[0];
                    ParentDirectory = topPathEntries[1];
                    GrandParentDirectory = topPathEntries[2];
                    break;
            }

            var dotEntries = new List<Tuple<IUnixDirectoryEntry, string>>();
            if (returnDotEntries)
            {
                dotEntries.Add(Tuple.Create(CurrentDirectory, "."));
                if (ParentDirectory != null)
                {
                    dotEntries.Add(Tuple.Create(ParentDirectory, ".."));
                }
            }

            _dotEntriesEnumerator = dotEntries.GetEnumerator();
            _entriesEnumerator = entries.GetEnumerator();
        }

        /// <summary>
        /// Gets the current directory.
        /// </summary>
        [NotNull]
        public IUnixDirectoryEntry CurrentDirectory { get; }

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        [CanBeNull]
        public IUnixDirectoryEntry ParentDirectory { get; }

        /// <summary>
        /// Gets the grand parent directory.
        /// </summary>
        [CanBeNull]
        public IUnixDirectoryEntry GrandParentDirectory { get; }

        /// <summary>
        /// Gets the name of the entry which might be different from the original entries name.
        /// </summary>
        [NotNull]
        public string Name
        {
            get
            {
                if (_enumerateDotEntries)
                {
                    Debug.Assert(_dotEntriesEnumerator.Current != null, "_dotEntriesEnumerator.Current != null");
                    return _dotEntriesEnumerator.Current.Item2;
                }

                Debug.Assert(_entriesEnumerator.Current != null, "_entriesEnumerator.Current != null");
                return _entriesEnumerator.Current.Name;
            }
        }

        /// <summary>
        /// Gets the file system entry.
        /// </summary>
        [NotNull]
        public IUnixFileSystemEntry Entry
        {
            get
            {
                if (_enumerateDotEntries)
                {
                    Debug.Assert(_dotEntriesEnumerator.Current != null, "_dotEntriesEnumerator.Current != null");
                    return _dotEntriesEnumerator.Current.Item1;
                }

                Debug.Assert(_entriesEnumerator.Current != null, "_entriesEnumerator.Current != null");
                return _entriesEnumerator.Current;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current entry is either the <c>.</c> or <c>..</c> entry.
        /// </summary>
        public bool IsDotEntry => _enumerateDotEntries;

        /// <summary>
        /// Called to enumerate the next directory listing entry.
        /// </summary>
        /// <returns><code>true</code> when there is a value for <see cref="Entry"/> and <see cref="Name"/>.</returns>
        public bool MoveNext()
        {
            if (_enumerateDotEntries)
            {
                if (_dotEntriesEnumerator.MoveNext())
                {
                    return true;
                }

                _enumerateDotEntries = false;
            }
            return _entriesEnumerator.MoveNext();
        }

        /// <summary>
        /// Gets the full path for a given name.
        /// </summary>
        /// <param name="name">The name to get the full path for.</param>
        /// <returns>The full path.</returns>
        public string GetFullPath(string name)
        {
            return _pathEntries.GetFullPath(name);
        }
    }
}
