// <copyright file="UnixFileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.Unix
{
    /// <summary>
    /// Options for the Unix file system.
    /// </summary>
    public class UnixFileSystemOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user ID should be changed during file system operations.
        /// </summary>
        public bool DisableUserIdSwitch { get; set; }
    }
}
