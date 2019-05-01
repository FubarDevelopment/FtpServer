// <copyright file="UnixFileSystemEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.AccountManagement;

using JetBrains.Annotations;

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal abstract class UnixFileSystemEntry : IUnixFileSystemEntry
    {
        private readonly UnixFileSystemInfo _info;

        protected UnixFileSystemEntry(
            [NotNull] UnixFileSystemInfo info)
        {
            _info = info;
            Permissions = new UnixPermissions(info);
        }

        /// <inheritdoc />
        public string Owner => _info.OwnerUser.UserName;

        /// <inheritdoc />
        public string Group => _info.OwnerGroup.GroupName;

        /// <inheritdoc />
        public string Name => _info.Name;

        /// <inheritdoc />
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc />
        public DateTimeOffset? LastWriteTime => _info.LastAccessTimeUtc;

        /// <inheritdoc />
        public DateTimeOffset? CreatedTime => _info.LastStatusChangeTimeUtc;

        /// <inheritdoc />
        public long NumberOfLinks => _info.LinkCount;
    }
}
