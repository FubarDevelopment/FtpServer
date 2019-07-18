//-----------------------------------------------------------------------
// <copyright file="DotNetFileEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    /// <summary>
    /// A <see cref="IUnixFileEntry"/> implementation for the standard
    /// .NET file system functionality.
    /// </summary>
    public class DotNetFileEntry : DotNetFileSystemEntry, IUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileEntry"/> class.
        /// </summary>
        /// <param name="info">The <see cref="FileInfo"/> to extract the information from.</param>
        public DotNetFileEntry(FileInfo info)
            : base(info)
        {
            FileInfo = info;
        }

        /// <summary>
        /// Gets the file information.
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <inheritdoc/>
        public long Size => FileInfo.Length;
    }
}
