// <copyright file="S3FileSystemEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer.FileSystem.AzureBlobStorage
{
    /// <summary>
    /// The basic file system entry for an azure blob storage file or directory.
    /// </summary>
    internal class AzureBlobStorageFileSystemEntry : IUnixFileSystemEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageFileSystemEntry"/> class.
        /// </summary>
        /// <param name="key">The S3-specific key.</param>
        /// <param name="name">The name of the entry.</param>
        public AzureBlobStorageFileSystemEntry(string key, string name)
        {
            Key = key;
            Name = name;
            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false));
        }

        /// <summary>
        /// Gets the S3-specific key.
        /// </summary>
        public string Key { get; }

        /// <inheritdoc />
        public string Owner => "owner";

        /// <inheritdoc />
        public string Group => "group";

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc />
        public DateTimeOffset? LastWriteTime { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? CreatedTime => null;

        /// <inheritdoc />
        public long NumberOfLinks => 1;
    }
}
