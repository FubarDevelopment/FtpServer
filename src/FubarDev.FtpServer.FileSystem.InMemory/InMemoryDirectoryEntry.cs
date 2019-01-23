// <copyright file="InMemoryDirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer.FileSystem.InMemory
{
    /// <summary>
    /// The im-memory directory entry.
    /// </summary>
    public class InMemoryDirectoryEntry : InMemoryFileSystemEntry, IUnixDirectoryEntry
    {
        private static readonly IUnixPermissions _defaultPermissions = new GenericUnixPermissions(
            new GenericAccessMode(true, true, true),
            new GenericAccessMode(true, true, true),
            new GenericAccessMode(true, false, true));

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDirectoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to.</param>
        /// <param name="parent">The parent entry.</param>
        /// <param name="name">The name of this entry.</param>
        /// <param name="children">The children of this directory entry.</param>
        public InMemoryDirectoryEntry(
            IUnixFileSystem fileSystem,
            InMemoryDirectoryEntry parent,
            string name,
            IDictionary<string, IUnixFileSystemEntry> children)
            : base(fileSystem, parent, name, _defaultPermissions)
        {
            Children = children;
        }

        /// <inheritdoc />
        public bool IsRoot => Parent is null;

        /// <inheritdoc />
        public bool IsDeletable => !IsRoot;

        /// <summary>
        /// Gets the children of this directory entry.
        /// </summary>
        public IDictionary<string, IUnixFileSystemEntry> Children { get; }
    }
}
