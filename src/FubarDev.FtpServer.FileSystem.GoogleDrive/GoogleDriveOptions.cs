// <copyright file="GoogleDriveOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// Options for the Google Drive-based file system.
    /// </summary>
    public class GoogleDriveOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether a background upload should be used.
        /// </summary>
        /// <remarks>
        /// If this is set to false, then the direct upload is used.
        /// </remarks>
        public bool UseBackgroundUpload { get; set; }
    }
}
