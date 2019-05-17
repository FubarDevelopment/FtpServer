// <copyright file="FileSystemGoogleDriveOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Common Google Drive options.
    /// </summary>
    public class FileSystemGoogleDriveOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether background upload should be used.
        /// </summary>
        public bool BackgroundUpload { get; set; }

        /// <summary>
        /// Gets or sets options for a Google Drive from a user.
        /// </summary>
        public FileSystemGoogleDriveUserOptions User { get; set; } = new FileSystemGoogleDriveUserOptions();

        /// <summary>
        /// Gets or sets options for a Google Drive for a service.
        /// </summary>
        public FileSystemGoogleDriveServiceOptions Service { get; set; } = new FileSystemGoogleDriveServiceOptions();
    }
}
