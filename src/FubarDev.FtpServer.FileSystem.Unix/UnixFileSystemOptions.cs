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
        /// Gets or sets the default root path.
        /// </summary>
        public string? Root { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content should be flushed to disk after every write operation.
        /// </summary>
        public bool FlushAfterWrite { get; set; }
    }
}
