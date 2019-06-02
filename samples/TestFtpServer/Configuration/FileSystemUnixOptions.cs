// <copyright file="FileSystemUnixOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

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
        [NotNull]
        public string Root { get; set; } = "/";
    }
}
