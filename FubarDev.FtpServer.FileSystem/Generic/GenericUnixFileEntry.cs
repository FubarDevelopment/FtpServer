//-----------------------------------------------------------------------
// <copyright file="GenericUnixFileEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    /// <summary>
    /// A generic implementation of the <see cref="IUnixFileEntry"/>
    /// </summary>
    public class GenericUnixFileEntry : GenericUnixFileSystemEntry, IUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericUnixFileEntry"/> class.
        /// </summary>
        /// <param name="name">The file name</param>
        /// <param name="size">The file size</param>
        /// <param name="permissions">The file permissions</param>
        /// <param name="lastWriteTime">The last write time</param>
        /// <param name="owner">The file owner</param>
        /// <param name="group">The file group</param>
        public GenericUnixFileEntry([NotNull] string name, long size, [NotNull] IUnixPermissions permissions, DateTimeOffset? lastWriteTime, [NotNull] string owner, [NotNull] string group)
            : base(name, permissions, lastWriteTime, owner, group)
        {
            Size = size;
        }

        /// <inheritdoc/>
        public long Size { get; }
    }
}
