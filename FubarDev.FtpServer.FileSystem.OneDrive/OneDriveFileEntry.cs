// <copyright file="OneDriveFileEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using RestSharp.Portable.Microsoft.OneDrive.Model;

namespace FubarDev.FtpServer.FileSystem.OneDrive
{
    internal class OneDriveFileEntry : OneDriveFileSystemEntry, IUnixFileEntry
    {
        public OneDriveFileEntry(OneDriveFileSystem fileSystem, Item item, long? fileSize)
            : base(fileSystem, item)
        {
            Size = fileSize ?? item.Size ?? 0;
        }

        public long Size { get; }
    }
}
