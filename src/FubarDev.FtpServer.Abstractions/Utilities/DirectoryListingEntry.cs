// <copyright file="DirectoryListingEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Utilities
{
 /// <summary>
    /// Entry returned by the <see cref="DirectoryListing"/>.
    /// </summary>
    public class DirectoryListingEntry
    {
        private readonly Lazy<string> _fullName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryListingEntry"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is only called for "dot"-Entries.
        /// </remarks>
        /// <param name="pathEntries">The entries for the current path (up to and including the current directory).</param>
        /// <param name="fileSystem">The file system this entry belongs to.</param>
        /// <param name="currentDirectory">The current directory, which may belong to a different file system.</param>
        /// <param name="parentDirectory">The parent directory.</param>
        /// <param name="grandParentDirectory">The grand parent directory.</param>
        /// <param name="entry">The current entry.</param>
        /// <param name="name">The alternative name.</param>
        public DirectoryListingEntry(
            Stack<IUnixDirectoryEntry> pathEntries,
            IUnixFileSystem fileSystem,
            IUnixDirectoryEntry currentDirectory,
            IUnixDirectoryEntry? parentDirectory,
            IUnixDirectoryEntry? grandParentDirectory,
            IUnixDirectoryEntry entry,
            string name)
        {
            FileSystem = fileSystem;
            CurrentDirectory = currentDirectory;
            ParentDirectory = parentDirectory;
            GrandParentDirectory = grandParentDirectory;
            Entry = entry;
            Name = name;
            IsDotEntry = true;
            _fullName = new Lazy<string>(() => pathEntries.GetFullPath(name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryListingEntry"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is called for all entries **except** "dot"-Entries.
        /// </remarks>
        /// <param name="pathEntries">The entries for the current path (up to and including the current directory).</param>
        /// <param name="fileSystem">The file system this entry belongs to.</param>
        /// <param name="currentDirectory">The current directory, which may belong to a different file system.</param>
        /// <param name="parentDirectory">The parent directory.</param>
        /// <param name="grandParentDirectory">The grand parent directory.</param>
        /// <param name="entry">The current entry.</param>
        public DirectoryListingEntry(
            Stack<IUnixDirectoryEntry> pathEntries,
            IUnixFileSystem fileSystem,
            IUnixDirectoryEntry currentDirectory,
            IUnixDirectoryEntry? parentDirectory,
            IUnixDirectoryEntry? grandParentDirectory,
            IUnixFileSystemEntry entry)
        {
            FileSystem = fileSystem;
            CurrentDirectory = currentDirectory;
            ParentDirectory = parentDirectory;
            GrandParentDirectory = grandParentDirectory;
            Entry = entry;
            Name = entry is IUnixDirectoryEntry dirEntry && dirEntry.IsRoot ? string.Empty : entry.Name;
            IsDotEntry = false;
            _fullName = new Lazy<string>(() => pathEntries.GetFullPath(entry.Name));
        }

        /// <summary>
        /// Gets the file system entry.
        /// </summary>
        public IUnixFileSystemEntry Entry { get; }

        /// <summary>
        /// Gets the name of the entry which might be different from the original entries name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the full path for the current entry.
        /// </summary>
        public string FullName => _fullName.Value;

        /// <summary>
        /// Gets a value indicating whether the current entry is either the <c>.</c> or <c>..</c> entry.
        /// </summary>
        public bool IsDotEntry { get; }

        /// <summary>
        /// Gets the file system of the entries to be enumerated.
        /// </summary>
        public IUnixFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the current directory.
        /// </summary>
        public IUnixDirectoryEntry CurrentDirectory { get; }

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        public IUnixDirectoryEntry? ParentDirectory { get; }

        /// <summary>
        /// Gets the grand parent directory.
        /// </summary>
        public IUnixDirectoryEntry? GrandParentDirectory { get; }
    }
}
