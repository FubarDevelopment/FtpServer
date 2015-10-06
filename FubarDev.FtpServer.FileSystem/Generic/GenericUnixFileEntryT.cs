//-----------------------------------------------------------------------
// <copyright file="GenericUnixFileEntryT.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    public class GenericUnixFileEntry<T> : GenericUnixFileEntry
    {
        public GenericUnixFileEntry(string name, long size, IUnixPermissions permissions, DateTimeOffset lastWriteTime, string owner, string @group, T opaque)
            : base(name, size, permissions, lastWriteTime, owner, @group)
        {
            Opaque = opaque;
        }

        public T Opaque { get; }
    }
}
