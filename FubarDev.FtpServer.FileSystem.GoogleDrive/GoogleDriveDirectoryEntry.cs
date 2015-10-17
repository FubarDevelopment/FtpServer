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
    internal class GoogleDriveDirectoryEntry : IUnixDirectoryEntry
    {
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

        public bool IsRoot { get; }

        /// <inheritdoc/>
        public bool IsDeletable => !IsRoot;

        public File File { get; }

        public string FullName { get; }

        public string Name { get; }

        public IUnixPermissions Permissions { get; }

        public DateTimeOffset? LastWriteTime => File.ModifiedByMeDate ?? File.ModifiedDate ?? File.CreatedDate;

        public long NumberOfLinks => 1;

        /// <inheritdoc/>
        public IUnixFileSystem FileSystem { get; }

        public string Owner => "owner";

        public string Group => "group";
    }
}
