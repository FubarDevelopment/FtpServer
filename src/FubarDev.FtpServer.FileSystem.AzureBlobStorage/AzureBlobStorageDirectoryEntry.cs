// <copyright file="S3DirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;


namespace FubarDev.FtpServer.FileSystem.AzureBlobStorage
{
    /// <summary>
    /// The virtual directory entry for blob objects.
    /// </summary>
    internal class AzureBlobStorageDirectoryEntry : AzureBlobStorageFileSystemEntry, IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageDirectoryEntry"/> class.
        /// </summary>
        /// <param name="key">The path/prefix of the child entries.</param>
        /// <param name="isRoot">Determines if this is the root directory.</param>
        public AzureBlobStorageDirectoryEntry(string key, bool isRoot = false)
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
