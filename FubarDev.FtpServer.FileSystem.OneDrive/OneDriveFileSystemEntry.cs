// <copyright file="OneDriveFileSystemEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.FileSystem.Generic;

using JetBrains.Annotations;

using RestSharp.Portable.Microsoft.OneDrive.Model;

namespace FubarDev.FtpServer.FileSystem.OneDrive
{
    internal class OneDriveFileSystemEntry : IUnixFileSystemEntry
    {
        public OneDriveFileSystemEntry([NotNull] OneDriveFileSystem fileSystem, [NotNull] Item item)
        {
            FileSystem = fileSystem;
            var isFolder = item.Folder != null;
            Item = item;
            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, isFolder),
                new GenericAccessMode(true, true, isFolder),
                new GenericAccessMode(true, true, isFolder));
        }

        public Item Item { get; }

        public string Group => "group";

        public DateTimeOffset? LastWriteTime => Item?.FileSystemInfo?.LastModifiedDateTime ?? Item?.LastModifiedDateTime ?? Item?.FileSystemInfo?.CreatedDateTime;

        public string Name => Item.ParentReference == null ? string.Empty : Item.Name;

        public long NumberOfLinks => 1;

        public string Owner => "owner";

        public IUnixPermissions Permissions { get; }

        public DateTimeOffset? CreatedTime => Item.FileSystemInfo?.CreatedDateTime ?? Item.CreatedDateTime;

        public IUnixFileSystem FileSystem { get; }
    }
}
