// <copyright file="FileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// System.IO based file system options.
    /// </summary>
    public class FileSystemSystemIoOptions
    {
        /// <summary>
        /// Gets or sets the root path.
        /// </summary>
        public string Root { get; set; } = Path.GetTempPath();
    }
}
