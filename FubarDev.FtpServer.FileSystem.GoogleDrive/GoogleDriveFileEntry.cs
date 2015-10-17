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
    internal class GoogleDriveFileEntry : IUnixFileEntry
    {
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

        public File File { get; }

        public string FullName { get; }

        public string Name => File.Title;

        public IUnixPermissions Permissions { get; }

        public DateTimeOffset? LastWriteTime => File.ModifiedByMeDate ?? File.ModifiedDate ?? File.CreatedDate;

        public long NumberOfLinks => 1;

        /// <inheritdoc/>
        public IUnixFileSystem FileSystem { get; }

        public string Owner => "owner";

        public string Group => "group";

        public long Size { get; }
    }
}
