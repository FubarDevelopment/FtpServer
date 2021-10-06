// <copyright file="S3FileSystemEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer.FileSystem.S3
{
    /// <summary>
    /// The basic file system entry for an S3 file or directory.
    /// </summary>
    internal class S3FileSystemEntry : IUnixFileSystemEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="S3FileSystemEntry"/> class.
        /// </summary>
        /// <param name="key">The S3-specific key.</param>
        /// <param name="name">The name of the entry.</param>
        public S3FileSystemEntry(string key, string name)
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
