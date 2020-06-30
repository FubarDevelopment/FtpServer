// <copyright file="InMemoryFileSystemEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;

namespace FubarDev.FtpServer.FileSystem.InMemory
{
    /// <summary>
    /// The base class for all in-memory file system entries.
    /// </summary>
    public abstract class InMemoryFileSystemEntry : IUnixFileSystemEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemEntry"/> class.
        /// </summary>
        /// <param name="parent">The parent entry.</param>
        /// <param name="name">The name of this entry.</param>
        /// <param name="permissions">The permissions of this entry.</param>
        protected InMemoryFileSystemEntry(
            InMemoryDirectoryEntry? parent,
            string name,
            IUnixPermissions permissions)
        {
            Name = name;
            Permissions = permissions;
            Parent = parent;
            Owner = "owner";
            Group = "group";
            CreatedTime = DateTimeOffset.Now;
        }

        /// <inheritdoc />
        public string Name { get; internal set; }

        /// <inheritdoc />
        public string Owner { get; private set; }

        /// <inheritdoc />
        public string Group { get; }

        /// <inheritdoc />
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc />
        public DateTimeOffset? LastWriteTime { get; private set; }

        /// <inheritdoc />
        public DateTimeOffset? CreatedTime { get; private set; }

        /// <inheritdoc />
        public long NumberOfLinks { get; } = 1;

        /// <summary>
        /// Gets or sets the parent entry.
        /// </summary>
        public InMemoryDirectoryEntry? Parent { get; set; }

        /// <summary>
        /// Configure directory entry as owned by given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user that becomes the new owner of this directory entry.</param>
        /// <returns>The changed file system entry.</returns>
        [Obsolete("Use the overload with ClaimsPrincipal.")]
        public InMemoryFileSystemEntry WithOwner(IFtpUser user)
        {
            Owner = user.Name;
            return this;
        }

        /// <summary>
        /// Configure directory entry as owned by given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user that becomes the new owner of this directory entry.</param>
        /// <returns>The changed file system entry.</returns>
        public InMemoryFileSystemEntry WithOwner(ClaimsPrincipal user)
        {
            Owner = user.Identity.Name;
            return this;
        }

        /// <summary>
        /// Sets the last write time.
        /// </summary>
        /// <param name="timestamp">The new value of the last write time.</param>
        /// <returns>The changed file system entry.</returns>
        public InMemoryFileSystemEntry SetLastWriteTime(DateTimeOffset timestamp)
        {
            LastWriteTime = timestamp;
            return this;
        }

        /// <summary>
        /// Sets the creation time.
        /// </summary>
        /// <param name="timestamp">The new value of the creation time.</param>
        /// <returns>The changed file system entry.</returns>
        public InMemoryFileSystemEntry SetCreateTime(DateTimeOffset timestamp)
        {
            CreatedTime = timestamp;
            return this;
        }
    }
}
