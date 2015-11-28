//-----------------------------------------------------------------------
// <copyright file="GoogleDriveDirectoryEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using FubarDev.FtpServer.FileSystem.Generic;

using JetBrains.Annotations;

using RestSharp.Portable.Google.Drive.Model;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// Implementatio of <see cref="IUnixDirectoryEntry"/> for Google Drive
    /// </summary>
    internal class GoogleDriveDirectoryEntry : IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveDirectoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this instance belongs to</param>
        /// <param name="file">The directory this entry belongs to</param>
        /// <param name="fullPath">The full path</param>
        /// <param name="isRoot">Is this a root directory?</param>
        public GoogleDriveDirectoryEntry([NotNull] GoogleDriveFileSystem fileSystem, [NotNull] File file, [NotNull] string fullPath, bool isRoot = false)
        {
            FileSystem = fileSystem;
            File = file;
            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, true),
                new GenericAccessMode(true, true, true),
                new GenericAccessMode(true, true, true));
            FullName = fullPath;
            IsRoot = isRoot;
            Name = File.Title;
        }

        /// <inheritdoc/>
        public bool IsRoot { get; }

        /// <inheritdoc/>
        public bool IsDeletable => !IsRoot;

        /// <summary>
        /// Gets the internal model data for Google Drive
        /// </summary>
        public File File { get; }

        /// <summary>
        /// Gets the full path relative to the drive root
        /// </summary>
        public string FullName { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc/>
        public DateTimeOffset? LastWriteTime => File.ModifiedByMeDate ?? File.ModifiedDate ?? File.CreatedDate;

        /// <inheritdoc/>
        public DateTimeOffset? CreatedTime => File.CreatedDate;

        /// <inheritdoc/>
        public long NumberOfLinks => 1;

        /// <inheritdoc/>
        public IUnixFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public string Owner => "owner";

        /// <inheritdoc/>
        public string Group => "group";
    }
}
