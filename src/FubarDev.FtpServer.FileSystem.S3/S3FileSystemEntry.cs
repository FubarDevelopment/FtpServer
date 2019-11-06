// <copyright file="S3FileSystemEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer.FileSystem.S3
{
    internal class S3FileSystemEntry : IUnixFileSystemEntry
    {
        public S3FileSystemEntry(string key, string name)
        {
            Key = key;
            Name = name;
            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false),
                new GenericAccessMode(true, true, false));
        }
        public string Key { get; }
        public string Owner => "owner";
        public string Group => "group";
        public string Name { get; }
        public IUnixPermissions Permissions { get; }
        public DateTimeOffset? LastWriteTime { get; set; }
        public DateTimeOffset? CreatedTime => null;
        public long NumberOfLinks => 1;
    }
}
