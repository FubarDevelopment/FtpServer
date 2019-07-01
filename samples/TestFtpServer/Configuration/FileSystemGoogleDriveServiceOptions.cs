// <copyright file="FileSystemGoogleDriveServiceOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for a Google Drive for a service.
    /// </summary>
    public class FileSystemGoogleDriveServiceOptions
    {
        /// <summary>
        /// Gets or sets the path to the credential file.
        /// </summary>
        [CanBeNull]
        public string CredentialFile { get; set; }
    }
}
