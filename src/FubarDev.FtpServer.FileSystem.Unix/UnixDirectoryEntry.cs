// <copyright file="UnixDirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal class UnixDirectoryEntry : UnixFileSystemEntry, IUnixDirectoryEntry
    {
        public UnixDirectoryEntry(
            UnixDirectoryInfo info,
            ClaimsPrincipal user,
            UnixUserInfo? userInfo,
            IUnixDirectoryEntry? parent = null)
            : base(info)
        {
            IsRoot = parent == null;
            Info = info;

            if (parent == null)
            {
                // Root user
                IsDeletable = false;
            }
            else if (info.Parent == info)
            {
                // File system root
                IsDeletable = false;
            }
            else if (userInfo != null && (userInfo.UserId == 0 || userInfo.GroupId == 0))
            {
                IsDeletable = true;
            }
            else
            {
                IsDeletable = parent.GetEffectivePermissions(user).Write;
            }
        }

        /// <summary>
        /// Gets the unix directory info.
        /// </summary>
        public UnixDirectoryInfo Info { get; }

        /// <inheritdoc />
        public bool IsRoot { get; }

        /// <inheritdoc />
        public bool IsDeletable { get; }
    }
}
