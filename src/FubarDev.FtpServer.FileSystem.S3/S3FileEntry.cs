// <copyright file="S3FileEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

namespace FubarDev.FtpServer.FileSystem.S3
{
    /// <summary>
    /// A file entry for an S3 object.
    /// </summary>
    internal class S3FileEntry : S3FileSystemEntry, IUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="S3FileEntry"/> class.
        /// </summary>
        /// <param name="key">The S3 object key.</param>
        /// <param name="size">The object size.</param>
        public S3FileEntry(string key, long size)
            : base(key, Path.GetFileName(key))
        {
            Size = size;
        }

        /// <inheritdoc />
        public long Size { get; }
    }
}
