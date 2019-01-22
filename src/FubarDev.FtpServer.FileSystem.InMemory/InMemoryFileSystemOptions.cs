// <copyright file="InMemoryFileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.FileSystem.InMemory
{
    /// <summary>
    /// In-memory file system options.
    /// </summary>
    public class InMemoryFileSystemOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the file system for an anonymous user should be kept.
        /// </summary>
        public bool KeepAnonymousFileSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file system for an authenticated user should be kept.
        /// </summary>
        public bool KeepAuthenticatedUserFileSystem { get; set; }

        /// <summary>
        /// Gets or sets the comparer for file system names.
        /// </summary>
        public StringComparer FileSystemComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Gets or sets the comparer for authenticated user names.
        /// </summary>
        public StringComparer UserNameComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Gets or sets the comparer for anonymous user e-mails.
        /// </summary>
        public StringComparer AnonymousComparer { get; set; } = StringComparer.OrdinalIgnoreCase;
    }
}
