//-----------------------------------------------------------------------
// <copyright file="GenericUnixDirectoryEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    public class GenericUnixDirectoryEntry : GenericUnixFileSystemEntry, IUnixDirectoryEntry
    {
        public GenericUnixDirectoryEntry(string name, IUnixPermissions permissions, DateTimeOffset lastWriteTime, string owner, string group)
            : base(name, permissions, lastWriteTime, owner, group)
        {
        }
    }
}
