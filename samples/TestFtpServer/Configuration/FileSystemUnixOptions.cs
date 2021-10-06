// <copyright file="FileSystemUnixOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Unix file system options.
    /// </summary>
    public class FileSystemUnixOptions
    {
        /// <summary>
        /// Gets or sets the root directory.
        /// </summary>
        public string Root { get; set; } = "/";

        /// <summary>
        /// Gets or sets a value indicating whether the content should be flushed to disk after every write operation.
        /// </summary>
        public bool FlushAfterWrite { get; set; }
    }
}
