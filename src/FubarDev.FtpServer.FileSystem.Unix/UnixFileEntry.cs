// <copyright file="UnixFileEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal class UnixFileEntry : UnixFileSystemEntry, IUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileEntry"/> class.
        /// </summary>
        /// <param name="info">The file information.</param>
        public UnixFileEntry([NotNull] UnixFileInfo info)
            : base(info)
        {
            Size = info.Length;
        }

        /// <inheritdoc />
        public long Size { get; }
    }
}
