// <copyright file="FileSystemInMemoryOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for the in-memory file system.
    /// </summary>
    public class FileSystemInMemoryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the in-memory file system should be kept between two connects.
        /// </summary>
        public bool KeepAnonymous { get; set; }
    }
}
