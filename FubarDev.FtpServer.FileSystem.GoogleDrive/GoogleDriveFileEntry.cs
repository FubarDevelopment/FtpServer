//-----------------------------------------------------------------------
// <copyright file="GoogleDriveFileEntry.cs" company="Fubar Development Junker">
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
    /// The implementation of <see cref="IUnixFileEntry"/> for Google Drive.
    /// </summary>
    internal class GoogleDriveFileEntry : IUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveFileEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="file">The underlying model</param>
        /// <param name="fullName">The full path and file name of this entry</param>
        /// <param name="fileSize">The file size (if it differs from the one in the model)</param>
        public GoogleDriveFileEntry([NotNull] GoogleDriveFileSystem fileSystem, [NotNull] File file, [NotNull] string fullName, long? fileSize = null)
        {
            FileSystem = fileSystem;
            File = file;
            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false));
            FullName = fullName;
            Size = fileSize ?? file.FileSize ?? 0;
        }

        /// <summary>
        /// Gets the underlying model
        /// </summary>
        public File File { get; }

        /// <summary>
        /// Gets the full path to this entry
        /// </summary>
        public string FullName { get; }

        /// <inheritdoc/>
        public string Name => File.Title;

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

        /// <inheritdoc/>
        public long Size { get; }
    }
}
