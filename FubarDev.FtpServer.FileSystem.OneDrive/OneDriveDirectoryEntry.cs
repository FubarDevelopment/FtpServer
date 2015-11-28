// <copyright file="OneDriveDirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

using RestSharp.Portable.Microsoft.OneDrive.Model;

namespace FubarDev.FtpServer.FileSystem.OneDrive
{
    internal class OneDriveDirectoryEntry : OneDriveFileSystemEntry, IUnixDirectoryEntry
    {
        public OneDriveDirectoryEntry([NotNull] OneDriveFileSystem fileSystem, [NotNull] Item folderItem, bool isRoot)
            : base(fileSystem, folderItem)
        {
            IsRoot = isRoot;
        }

        /// <inheritdoc/>
        public bool IsRoot { get; }

        /// <inheritdoc/>
        public bool IsDeletable => false;
    }
}
