//-----------------------------------------------------------------------
// <copyright file="IUnixFileSystemEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// The file system entry information that is shared between a <see cref="IUnixDirectoryEntry"/> and a <see cref="IUnixFileEntry"/>.
    /// </summary>
    public interface IUnixFileSystemEntry : IUnixOwner
    {
        /// <summary>
        /// Gets the name of the file system entry.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the file entry permissions.
        /// </summary>
        IUnixPermissions Permissions { get; }

        /// <summary>
        /// Gets the last write time.
        /// </summary>
        DateTimeOffset? LastWriteTime { get; }

        /// <summary>
        /// Gets the time of creation.
        /// </summary>
        DateTimeOffset? CreatedTime { get; }

        /// <summary>
        /// Gets the number of links.
        /// </summary>
        long NumberOfLinks { get; }
    }
}
