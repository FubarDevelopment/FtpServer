// <copyright file="UnixFileSystemEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Globalization;

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal abstract class UnixFileSystemEntry : IUnixFileSystemEntry
    {
        private readonly UnixFileSystemInfo _info;

        protected UnixFileSystemEntry(
            UnixFileSystemInfo info)
        {
            GenericInfo = _info = info;
            Permissions = new UnixPermissions(info);
            Owner = GetNameOrId(() => info.OwnerUser.UserName, () => info.OwnerUserId);
            Group = GetNameOrId(() => info.OwnerGroup.GroupName, () => info.OwnerGroupId);
        }

        /// <summary>
        /// Gets generic unix file system entry information.
        /// </summary>
        public UnixFileSystemInfo GenericInfo { get; }

        /// <inheritdoc />
        public string Owner { get; }

        /// <inheritdoc />
        public string Group { get; }

        /// <inheritdoc />
        public string Name => _info.Name;

        /// <inheritdoc />
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc />
        public DateTimeOffset? LastWriteTime => _info.LastWriteTimeUtc;

        /// <inheritdoc />
        public DateTimeOffset? CreatedTime => _info.LastStatusChangeTimeUtc;

        /// <inheritdoc />
        public long NumberOfLinks => _info.LinkCount;

        private static string GetNameOrId(Func<string> getName, Func<long> getId)
        {
            try
            {
                return getName();
            }
            catch
            {
                return getId().ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
