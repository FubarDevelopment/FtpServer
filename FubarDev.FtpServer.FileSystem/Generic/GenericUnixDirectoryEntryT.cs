//-----------------------------------------------------------------------
// <copyright file="GenericUnixDirectoryEntryT.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    public class GenericUnixDirectoryEntry<T> : GenericUnixDirectoryEntry
    {
        public GenericUnixDirectoryEntry(string name, IUnixPermissions permissions, DateTimeOffset lastWriteTime, string owner, string @group, T opaque)
            : base(name, permissions, lastWriteTime, owner, @group)
        {
            Opaque = opaque;
        }

        public T Opaque { get; }
    }
}
