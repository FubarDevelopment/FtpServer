//-----------------------------------------------------------------------
// <copyright file="GenericUnixDirectoryEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    /// <summary>
    /// A generic implementation of the <see cref="IUnixDirectoryEntry"/>
    /// </summary>
    public class GenericUnixDirectoryEntry : GenericUnixFileSystemEntry, IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericUnixDirectoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="name">The directory name</param>
        /// <param name="permissions">The directory permissions</param>
        /// <param name="lastWriteTime">The last write time</param>
        /// <param name="owner">The owner</param>
        /// <param name="group">The group</param>
        public GenericUnixDirectoryEntry([NotNull] IUnixFileSystem fileSystem, [NotNull] string name, [NotNull] IUnixPermissions permissions, DateTimeOffset? lastWriteTime, [NotNull] string owner, [NotNull] string group)
            : base(fileSystem, name, permissions, lastWriteTime, owner, group)
        {
        }

        /// <inheritdoc/>
        public bool IsRoot => string.IsNullOrEmpty(Name);

        /// <inheritdoc/>
        public bool IsDeletable => !IsRoot;
    }
}
