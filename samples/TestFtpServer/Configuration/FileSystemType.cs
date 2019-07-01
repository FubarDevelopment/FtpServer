// <copyright file="FileSystemType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The file system to use.
    /// </summary>
    public enum FileSystemType
    {
        /// <summary>
        /// The System.IO based file system.
        /// </summary>
        SystemIO,

        /// <summary>
        /// A file system that uses the native Linux API.
        /// </summary>
        Unix,

        /// <summary>
        /// In-Memory file system.
        /// </summary>
        InMemory,

        /// <summary>
        /// Google Drive for a user.
        /// </summary>
        GoogleDriveUser,

        /// <summary>
        /// Google Drive for a service.
        /// </summary>
        GoogleDriveService,
    }
}
