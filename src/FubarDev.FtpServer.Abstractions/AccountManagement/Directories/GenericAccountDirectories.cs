// <copyright file="GenericAccountDirectories.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.AccountManagement.Directories
{
    /// <summary>
    /// Default implementation of <see cref="IAccountDirectories"/>.
    /// </summary>
    public class GenericAccountDirectories : IAccountDirectories
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericAccountDirectories"/> class.
        /// </summary>
        /// <param name="rootPath">The root path relative to the file systems root path.</param>
        /// <param name="homePath">The home directory of the user relative to the <paramref name="rootPath"/>.</param>
        public GenericAccountDirectories(
            string? rootPath,
            string? homePath = null)
        {
            RootPath = rootPath?.RemoveRoot();
            HomePath = homePath?.RemoveRoot();
        }

        /// <inheritdoc />
        public string? RootPath { get; }

        /// <inheritdoc />
        public string? HomePath { get; }
    }
}
