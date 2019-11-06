// <copyright file="S3FileEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

namespace FubarDev.FtpServer.FileSystem.S3
{
    internal class S3FileEntry : S3FileSystemEntry, IUnixFileEntry
    {
        public S3FileEntry(string key)
        : base(key, Path.GetFileName(key))
        {
        }

        public long Size { get; set; }
    }
}
