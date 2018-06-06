//-----------------------------------------------------------------------
// <copyright file="IUnixFileEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// A unix file entry.
    /// </summary>
    public interface IUnixFileEntry : IUnixFileSystemEntry
    {
        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        long Size { get; }
    }
}
