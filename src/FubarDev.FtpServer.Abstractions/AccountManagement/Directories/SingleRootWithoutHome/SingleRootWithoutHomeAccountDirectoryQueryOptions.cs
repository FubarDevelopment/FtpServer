// <copyright file="SingleRootWithoutHomeAccountDirectoryQueryOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome
{
    /// <summary>
    /// Options for the <see cref="SingleRootWithoutHomeAccountDirectoryQuery"/>.
    /// </summary>
    public class SingleRootWithoutHomeAccountDirectoryQueryOptions
    {
        /// <summary>
        /// Gets or sets the root path.
        /// </summary>
        [CanBeNull]
        public string RootPath { get; set; }
    }
}
