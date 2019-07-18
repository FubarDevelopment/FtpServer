// <copyright file="PamAccountDirectoryQueryOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.MembershipProvider.Pam.Directories
{
    /// <summary>
    /// Options for the PAM account directory query.
    /// </summary>
    public class PamAccountDirectoryQueryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the users home directory is its root.
        /// </summary>
        public bool UserHomeIsRoot { get; set; }

        /// <summary>
        /// Gets or sets the anonymous root directory.
        /// </summary>
        /// <remarks>
        /// This property must be set to allow anonymous users.
        /// This path is relative to the file systems root path.
        /// </remarks>
        public string? AnonymousRootDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether anonymous users should have their own (per-email) root directory.
        /// </summary>
        public bool AnonymousRootPerEmail { get; set; }
    }
}
