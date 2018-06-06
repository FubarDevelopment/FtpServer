//-----------------------------------------------------------------------
// <copyright file="IUnixDirectoryEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// A unix directory entry.
    /// </summary>
    public interface IUnixDirectoryEntry : IUnixFileSystemEntry
    {
        /// <summary>
        /// Gets a value indicating whether this is the root directory.
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        /// Gets a value indicating whether this directory can be deleted.
        /// </summary>
        bool IsDeletable { get; }
    }
}
