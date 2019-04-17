//-----------------------------------------------------------------------
// <copyright file="DotNetDirectoryEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Linq;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    /// <summary>
    /// A <see cref="IUnixDirectoryEntry"/> implementation for the standard
    /// .NET file system functionality.
    /// </summary>
    public class DotNetDirectoryEntry : DotNetFileSystemEntry, IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetDirectoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to.</param>
        /// <param name="dirInfo">The <see cref="DirectoryInfo"/> to extract the information from.</param>
        /// <param name="isRoot">Defines whether this the root directory.</param>
        public DotNetDirectoryEntry([NotNull] DotNetFileSystem fileSystem, [NotNull] DirectoryInfo dirInfo, bool isRoot)
            : base(fileSystem, dirInfo)
        {
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
        public bool IsDeletable => !IsRoot && (FileSystem.SupportsNonEmptyDirectoryDelete || !DirectoryInfo.EnumerateFileSystemInfos().Any());
    }
}
