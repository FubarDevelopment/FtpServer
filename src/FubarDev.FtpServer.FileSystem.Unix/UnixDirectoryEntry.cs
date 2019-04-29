// <copyright file="UnixDirectoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal class UnixDirectoryEntry : UnixFileSystemEntry, IUnixDirectoryEntry
    {
        [NotNull]
        private readonly UnixDirectoryInfo _info;

        private readonly UnixUserInfo _userInfo;

        /// <inheritdoc />
        public UnixDirectoryEntry(
            [NotNull] UnixDirectoryInfo info,
            bool isRoot,
            UnixUserInfo userInfo)
            : base(info)
        {
            _info = info;
            _userInfo = userInfo;
            IsRoot = isRoot;
        }

        /// <inheritdoc />
        public bool IsRoot { get; }

        /// <inheritdoc />
        public bool IsDeletable
        {
            get
            {
                if (IsRoot)
                {
                    return false;
                }
            }
        }
    }
}
