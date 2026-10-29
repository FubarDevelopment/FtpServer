// <copyright file="S3FileEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

namespace FubarDev.FtpServer.FileSystem.AzureBlobStorage
{
    /// <summary>
    /// A file entry for an azure blob storage object.
    /// </summary>
    internal class AzureBlobStorageFileEntry : AzureBlobStorageFileSystemEntry, IUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageFileEntry"/> class.
        /// </summary>
        /// <param name="key">The blob object key.</param>
        /// <param name="size">The object size.</param>
        public AzureBlobStorageFileEntry(string key, long size)
            : base(key, Path.GetFileName(key))
        {
            Size = size;
        }

        /// <inheritdoc />
        public long Size { get; }
    }
}
