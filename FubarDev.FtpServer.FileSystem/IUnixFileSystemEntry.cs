//-----------------------------------------------------------------------
// <copyright file="IUnixFileSystemEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.FileSystem
{
    public interface IUnixFileSystemEntry
    {
        string Name { get; }

        IUnixPermissions Permissions { get; }

        DateTimeOffset? LastWriteTime { get; }

        long NumberOfLinks { get; }

        string Owner { get; }

        string Group { get; }
    }
}
