//-----------------------------------------------------------------------
// <copyright file="GenericUnixFileEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    public class GenericUnixFileEntry : GenericUnixFileSystemEntry, IUnixFileEntry
    {
        public GenericUnixFileEntry(string name, long size, IUnixPermissions permissions, DateTimeOffset lastWriteTime, string owner, string group)
            : base(name, permissions, lastWriteTime, owner, group)
        {
            Size = size;
        }

        public long Size { get; }
    }
}
