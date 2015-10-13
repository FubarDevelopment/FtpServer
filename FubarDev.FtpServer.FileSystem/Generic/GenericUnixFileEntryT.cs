//-----------------------------------------------------------------------
// <copyright file="GenericUnixFileEntryT.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.Generic
{
    /// <summary>
    /// A generic implementation of the <see cref="IUnixFileEntry"/> interface.
    /// </summary>
    /// <typeparam name="T">The underlying data type for the <see cref="Opaque"/> property</typeparam>
    public class GenericUnixFileEntry<T> : GenericUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericUnixFileEntry{T}"/> class.
        /// </summary>
        /// <param name="name">The file name</param>
        /// <param name="size">The file size</param>
        /// <param name="permissions">The file permissions</param>
        /// <param name="lastWriteTime">The last write time</param>
        /// <param name="owner">The file owner</param>
        /// <param name="group">The file group</param>
        /// <param name="opaque">The underlying data of type <typeparamref name="T"/></param>
        public GenericUnixFileEntry([NotNull] string name, long size, [NotNull] IUnixPermissions permissions, DateTimeOffset? lastWriteTime, [NotNull] string owner, [NotNull] string @group, T opaque)
            : base(name, size, permissions, lastWriteTime, owner, @group)
        {
            Opaque = opaque;
        }

        /// <summary>
        /// Gets the underlying data
        /// </summary>
        public T Opaque { get; }
    }
}
