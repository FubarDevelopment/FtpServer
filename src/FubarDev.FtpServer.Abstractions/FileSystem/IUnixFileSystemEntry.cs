//-----------------------------------------------------------------------
// <copyright file="IUnixFileSystemEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

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
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets the file entry permissions.
        /// </summary>
        [NotNull]
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

        /// <summary>
        /// Gets the file system this entry belongs to.
        /// </summary>
        [NotNull]
        IUnixFileSystem FileSystem { get; }
    }
}
