//-----------------------------------------------------------------------
// <copyright file="GenericUnixFileSystemEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    public abstract class GenericUnixFileSystemEntry : IUnixFileSystemEntry
    {
        public GenericUnixFileSystemEntry(string name, IUnixPermissions permissions, DateTimeOffset lastWriteTime, string owner, string group)
        {
            Name = name;
            Permissions = permissions;
            LastWriteTime = lastWriteTime;
            Owner = owner;
            Group = group;
            NumberOfLinks = 1;
        }

        public string Name { get; }

        public IUnixPermissions Permissions { get; }

        public DateTimeOffset? LastWriteTime { get; }

        public long NumberOfLinks { get; }

        public string Owner { get; }

        public string Group { get; }
    }
}
