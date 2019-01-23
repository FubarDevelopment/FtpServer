// <copyright file="IGoogleDriveFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Google.Apis.Drive.v3;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// Interface that needs to be implemented by a Google Drive-backed file system.
    /// </summary>
    internal interface IGoogleDriveFileSystem : IUnixFileSystem
    {
        /// <summary>
        /// Gets the <see cref="DriveService"/> instance to use to access the Google Drive.
        /// </summary>
        [NotNull]
        DriveService Service { get; }

        /// <summary>
        /// Notification when the background upload for the given file is finished.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        void UploadFinished(string fileId);
    }
}
