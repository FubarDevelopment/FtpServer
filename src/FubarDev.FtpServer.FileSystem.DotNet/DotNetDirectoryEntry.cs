//-----------------------------------------------------------------------
// <copyright file="DotNetDirectoryEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Linq;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    /// <summary>
    /// A <see cref="IUnixDirectoryEntry"/> implementation for the standard
    /// .NET file system functionality.
    /// </summary>
    public class DotNetDirectoryEntry : DotNetFileSystemEntry, IUnixDirectoryEntry
    {
        private readonly bool _allowDeleteIfNotEmpty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetDirectoryEntry"/> class.
        /// </summary>
        /// <param name="dirInfo">The <see cref="DirectoryInfo"/> to extract the information from.</param>
        /// <param name="isRoot">Defines whether this the root directory.</param>
        /// <param name="allowDeleteIfNotEmpty">Is deletion of this directory allowed if it's not empty.</param>
        public DotNetDirectoryEntry(DirectoryInfo dirInfo, bool isRoot, bool allowDeleteIfNotEmpty)
            : base(dirInfo)
        {
            _allowDeleteIfNotEmpty = allowDeleteIfNotEmpty;
            IsRoot = isRoot;
            DirectoryInfo = dirInfo;
        }

        /// <summary>
        /// Gets the directory information.
        /// </summary>
        public DirectoryInfo DirectoryInfo { get; }

        /// <inheritdoc/>
        public bool IsRoot { get; }

        /// <inheritdoc/>
        public bool IsDeletable => !IsRoot && (_allowDeleteIfNotEmpty || !DirectoryInfo.EnumerateFileSystemInfos().Any());
    }
}
