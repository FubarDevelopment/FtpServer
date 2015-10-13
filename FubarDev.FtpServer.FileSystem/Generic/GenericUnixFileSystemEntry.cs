//-----------------------------------------------------------------------
// <copyright file="GenericUnixFileSystemEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    /// <summary>
    /// A generic implementation of the <see cref="IUnixFileSystemEntry"/>
    /// </summary>
    public abstract class GenericUnixFileSystemEntry : IUnixFileSystemEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericUnixFileSystemEntry"/> class.
        /// </summary>
        /// <param name="name">The file system entry name</param>
        /// <param name="permissions">The file system entry permissions</param>
        /// <param name="lastWriteTime">The last write time</param>
        /// <param name="owner">The file system entry owner</param>
        /// <param name="group">The file system entry group</param>
        protected GenericUnixFileSystemEntry([NotNull] string name, [NotNull] IUnixPermissions permissions, DateTimeOffset? lastWriteTime, [NotNull] string owner, [NotNull] string group)
        {
            Name = name;
            Permissions = permissions;
            LastWriteTime = lastWriteTime;
            Owner = owner;
            Group = group;
            NumberOfLinks = 1;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc/>
        public DateTimeOffset? LastWriteTime { get; }

        /// <inheritdoc/>
        public long NumberOfLinks { get; }

        /// <inheritdoc/>
        public string Owner { get; }

        /// <inheritdoc/>
        public string Group { get; }
    }
}
