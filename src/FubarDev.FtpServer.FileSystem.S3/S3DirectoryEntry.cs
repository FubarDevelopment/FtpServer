// <copyright file="S3DirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

namespace FubarDev.FtpServer.FileSystem.S3
{
    internal class S3DirectoryEntry : S3FileSystemEntry, IUnixDirectoryEntry
    {
        public S3DirectoryEntry(string key, bool isRoot = false)
            : base(key.EndsWith("/") || isRoot ? key : key + "/", Path.GetFileName(key.TrimEnd('/')))
        {
            IsRoot = isRoot;
        }

        public bool IsRoot { get; }
        public bool IsDeletable => !IsRoot;
    }
}
