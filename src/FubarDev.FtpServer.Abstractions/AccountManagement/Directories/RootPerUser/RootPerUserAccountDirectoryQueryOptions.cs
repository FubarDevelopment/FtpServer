// <copyright file="RootPerUserAccountDirectoryQueryOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.ComponentModel;

namespace FubarDev.FtpServer.AccountManagement.Directories.RootPerUser
{
    /// <summary>
    /// Options for the <see cref="RootPerUserAccountDirectoryQuery"/>.
    /// </summary>
    public class RootPerUserAccountDirectoryQueryOptions
    {
        /// <summary>
        /// Gets or sets the normal authenticated users root directory.
        /// </summary>
        /// <remarks>
        /// This path is relative to the file systems root path.
        /// </remarks>
        public string? UserRootDirectory { get; set; }

        /// <summary>
        /// Gets or sets the anonymous root directory.
        /// </summary>
        /// <remarks>
        /// Anonymous users will have the root <c>anonymous</c> if this
        /// property isn't set.
        /// This path is relative to the file systems root path.
        /// </remarks>
        [DefaultValue("anonymous")]
        public string? AnonymousRootDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether anonymous users should have their own (per-email) root directory.
        /// </summary>
        public bool AnonymousRootPerEmail { get; set; }
    }
}
