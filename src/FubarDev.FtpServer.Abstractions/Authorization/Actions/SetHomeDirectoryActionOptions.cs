// <copyright file="SetHomeDirectoryActionOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Authorization.Actions
{
    /// <summary>
    /// Options for the <see cref="SetHomeDirectoryAction"/>.
    /// </summary>
    public class SetHomeDirectoryActionOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether missing home directories should be created.
        /// </summary>
        public bool CreateMissingDirectories { get; set; }
    }
}
