// <copyright file="GoogleDriveDirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.FileSystem.Generic;

using Google.Apis.Drive.v3.Data;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
   /// <summary>
    /// Implementation of <see cref="IUnixDirectoryEntry"/> for Google Drive.
    /// </summary>
    internal class GoogleDriveDirectoryEntry : IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveDirectoryEntry"/> class.
        /// </summary>
        /// <param name="file">The directory this entry belongs to.</param>
        /// <param name="fullPath">The full path.</param>
        /// <param name="isRoot">Determines whether this a root directory.</param>
        public GoogleDriveDirectoryEntry(File file, string fullPath, bool isRoot = false)
        {
            File = file;
            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, true),
                new GenericAccessMode(true, true, true),
                new GenericAccessMode(true, true, true));
            FullName = fullPath;
            IsRoot = isRoot;
            Name = File.Name;
            NumberOfLinks = File.Parents?.Count ?? 1;
        }

        /// <inheritdoc/>
        public bool IsRoot { get; }

        /// <inheritdoc/>
        public bool IsDeletable => !IsRoot;

        /// <summary>
        /// Gets the internal model data for Google Drive.
        /// </summary>
        public File File { get; }

        /// <summary>
        /// Gets the full path relative to the drive root.
        /// </summary>
        public string FullName { get; }

        /// <inheritdoc/>
        public string Name { get; }

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
    }
}
