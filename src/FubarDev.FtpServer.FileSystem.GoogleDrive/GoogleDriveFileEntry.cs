// <copyright file="GoogleDriveFileEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Principal;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.Generic;

using Google.Apis.Drive.v3.Data;

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
        /// <param name="file">The underlying model.</param>
        /// <param name="fullName">The full path and file name of this entry.</param>
        /// <param name="fileSize">The file size (if it differs from the one in the model).</param>
        public GoogleDriveFileEntry(
            File file,
            string fullName,
            long? fileSize = null)
        {
            File = file;
            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false));
            FullName = fullName;
            Size = fileSize ?? file.Size ?? 0;
            NumberOfLinks = File.Parents?.Count ?? 1;
        }

        /// <summary>
        /// Gets the underlying model.
        /// </summary>
        public File File { get; }

        /// <summary>
        /// Gets the full path to this entry.
        /// </summary>
        public string FullName { get; }

        /// <inheritdoc/>
        public string Name => File.Name;

        /// <inheritdoc/>
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc/>
        public DateTimeOffset? LastWriteTime => File.ModifiedByMeTime ?? File.ModifiedTime ?? File.CreatedTime;

        /// <inheritdoc/>
        public DateTimeOffset? CreatedTime => File.CreatedTime;

        /// <inheritdoc/>
        public long NumberOfLinks { get; }

        /// <inheritdoc/>
        public string Owner => "owner";

        /// <inheritdoc/>
        public string Group => "group";

        /// <inheritdoc/>
        public long Size { get; }

        public IPrincipal PrincipalUser => throw new NotImplementedException();

        public IFtpUser User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
