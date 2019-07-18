// <copyright file="IRenameCommandFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Feature for the <c>RNFR</c> and <c>RNTO</c> commands.
    /// </summary>
    public interface IRenameCommandFeature
    {
        /// <summary>
        /// Gets or sets the <see cref="IUnixFileEntry"/> to use for a <c>RNTO</c> operation.
        /// </summary>
        SearchResult<IUnixFileSystemEntry> RenameFrom { get; set; }
    }
}
