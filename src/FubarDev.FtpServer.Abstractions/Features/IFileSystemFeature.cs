// <copyright file="IFileSystemFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// File system feature.
    /// </summary>
    public interface IFileSystemFeature
    {
        /// <summary>
        /// Gets or sets the <see cref="IUnixFileSystem"/> to use for the user.
        /// </summary>
        IUnixFileSystem FileSystem { get; set; }

        /// <summary>
        /// Gets or sets the current path into the <see cref="FileSystem"/>.
        /// </summary>
        Stack<IUnixDirectoryEntry> Path { get; set; }

        /// <summary>
        /// Gets the current <see cref="IUnixDirectoryEntry"/> of the current <see cref="Path"/>.
        /// </summary>
        IUnixDirectoryEntry CurrentDirectory { get; }
    }
}
