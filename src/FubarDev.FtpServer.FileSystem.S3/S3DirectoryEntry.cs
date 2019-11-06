// <copyright file="S3DirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

namespace FubarDev.FtpServer.FileSystem.S3
{
    /// <summary>
    /// The virtual directory entry for S3 objects.
    /// </summary>
    /// <remarks>
    /// S3 doesn't know directories, just objects with keys, that may or may not be delimited by slashes.
    /// </remarks>
    internal class S3DirectoryEntry : S3FileSystemEntry, IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="S3DirectoryEntry"/> class.
        /// </summary>
        /// <param name="key">The path/prefix of the child entries.</param>
        /// <param name="isRoot">Determines if this is the root directory.</param>
        public S3DirectoryEntry(string key, bool isRoot = false)
            : base(key.EndsWith("/") || isRoot ? key : key + "/", Path.GetFileName(key.TrimEnd('/')))
        {
            IsRoot = isRoot;
        }

        /// <inheritdoc />
        public bool IsRoot { get; }

        /// <inheritdoc />
        public bool IsDeletable => !IsRoot;
    }
}
