// <copyright file="DotNetFileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    /// <summary>
    /// Options for the .NET API based file system access.
    /// </summary>
    public class DotNetFileSystemOptions
    {
        /// <summary>
        /// Gets or sets the root path for all users.
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user ID should be used as sub directory.
        /// </summary>
        public bool UseUserIdAsSubFolder { get; set; }

        /// <summary>
        /// Gets or sets the buffer size to be used in async IO methods.
        /// </summary>
        public int? StreamBufferSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether deletion of non-empty directories is allowed.
        /// </summary>
        public bool AllowNonEmptyDirectoryDelete { get; set; }
    }
}
