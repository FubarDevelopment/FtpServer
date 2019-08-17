// <copyright file="IAccountDirectories.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Well-known directories for a given account.
    /// </summary>
    public interface IAccountDirectories
    {
        /// <summary>
        /// Gets the FTP root path.
        /// </summary>
        /// <remarks>
        /// The root path is <b>always</b> relative to the file system root path.
        /// If this path is not set, the file systems root directory will be used.
        /// </remarks>
        string? RootPath { get; }

        /// <summary>
        /// Gets the initial path for the account.
        /// </summary>
        /// <remarks>
        /// This path is always relative to the root path above.
        /// <c>/</c> will be used if the <see cref="RootPath"/>
        /// is not set.
        /// </remarks>
        string? HomePath { get; }
    }
}
