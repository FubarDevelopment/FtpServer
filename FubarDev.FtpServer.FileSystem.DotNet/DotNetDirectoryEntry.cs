//-----------------------------------------------------------------------
// <copyright file="DotNetDirectoryEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;

using FubarDev.FtpServer.FileSystem.Generic;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    /// <summary>
    /// A <see cref="IUnixDirectoryEntry"/> implementation for the standard
    /// .NET file system functionality.
    /// </summary>
    public class DotNetDirectoryEntry : IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetDirectoryEntry"/> class.
        /// </summary>
        /// <param name="dirInfo">The <see cref="DirectoryInfo"/> to extract the information from</param>
        public DotNetDirectoryEntry(DirectoryInfo dirInfo)
        {
            Info = dirInfo;
            LastWriteTime = new DateTimeOffset(Info.LastWriteTime);
            var accessMode = new GenericAccessMode(true, true, true);
            Permissions = new GenericUnixPermissions(accessMode, accessMode, accessMode);
        }

        /// <summary>
        /// Gets the underlying <see cref="DirectoryInfo"/>
        /// </summary>
        public DirectoryInfo Info { get; }

        /// <inheritdoc/>
        public string Name => Info.Name;

        /// <inheritdoc/>
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc/>
        public DateTimeOffset? LastWriteTime { get; }

        /// <inheritdoc/>
        public long NumberOfLinks => 1;

        /// <inheritdoc/>
        public string Owner => "owner";

        /// <inheritdoc/>
        public string Group => "group";
    }
}
