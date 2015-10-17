//-----------------------------------------------------------------------
// <copyright file="GenericUnixDirectoryEntryT.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    /// <summary>
    /// A generic implementation of the <see cref="IUnixDirectoryEntry"/> interface.
    /// </summary>
    /// <typeparam name="T">The underlying data type for the <see cref="Opaque"/> property</typeparam>
    public class GenericUnixDirectoryEntry<T> : GenericUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericUnixDirectoryEntry{T}"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="name">The directory name</param>
        /// <param name="permissions">The directory permissions</param>
        /// <param name="lastWriteTime">The last write time</param>
        /// <param name="owner">The owner</param>
        /// <param name="group">The group</param>
        /// <param name="opaque">The underlying data of type <typeparamref name="T"/></param>
        public GenericUnixDirectoryEntry([NotNull] IUnixFileSystem fileSystem, string name, IUnixPermissions permissions, DateTimeOffset lastWriteTime, string owner, string @group, T opaque)
            : base(fileSystem, name, permissions, lastWriteTime, owner, @group)
        {
            Opaque = opaque;
        }

        /// <summary>
        /// Gets the underlying data
        /// </summary>
        public T Opaque { get; }
    }
}
