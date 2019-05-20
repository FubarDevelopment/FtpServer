// <copyright file="FtpOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using Newtonsoft.Json;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The root object for all options.
    /// </summary>
    public class FtpOptions
    {
        private FileSystemLayoutType? _layout;
        private FileSystemType _backend = FileSystemType.InMemory;

        /// <summary>
        /// Gets or sets authentication providers to use.
        /// </summary>
        public MembershipProviderType Authentication { get; set; } = MembershipProviderType.Default;

        /// <summary>
        /// Gets or sets the PAM authorization options.
        /// </summary>
        public PamAuthOptions Pam { get; set; } = new PamAuthOptions();

        /// <summary>
        /// Gets or sets the FTP server options.
        /// </summary>
        public FtpServerOptions Server { get; set; } = new FtpServerOptions();

        /// <summary>
        /// Gets or sets the FTPS options.
        /// </summary>
        public FtpsOptions Ftps { get; set; } = new FtpsOptions();

        /// <summary>
        /// Gets or sets the file system backend to use.
        /// </summary>
        public string Backend
        {
            get => _backend switch {
                FileSystemType.InMemory => "in-memory",
                FileSystemType.SystemIO => "system-io",
                FileSystemType.Unix => "unix",
                FileSystemType.GoogleDriveUser => "google-drive:user",
                FileSystemType.GoogleDriveService => "google-drive:service",
                _ => "in-memory",
                };
            set => _backend = value switch {
                "in-memory" => FileSystemType.InMemory,
                "system-io" => FileSystemType.SystemIO,
                "unix" => FileSystemType.Unix,
                "google-drive:user" => FileSystemType.GoogleDriveUser,
                "google-drive:service" => FileSystemType.GoogleDriveService,
                _ => throw new ArgumentOutOfRangeException(
                    $"Value must be one of \"in-memory\", \"system-io\", \"unix\", \"google-drive:user\", \"google-drive:service\", but was , \"{value}\"")
                };
        }

        /// <summary>
        /// Gets or sets the file system layout to use.
        /// </summary>
        public string Layout
        {
            get => _layout switch {
                FileSystemLayoutType.SingleRoot => "single-root",
                FileSystemLayoutType.RootPerUser => "root-per-user",
                FileSystemLayoutType.PamHome => "pam-home",
                FileSystemLayoutType.PamHomeChroot => "pam-home-chroot",
                _ => "single-root"
                };
            set => _layout = value switch {
                "default" => (FileSystemLayoutType?)null,
                "single-root" => FileSystemLayoutType.SingleRoot,
                "root-per-user" => FileSystemLayoutType.RootPerUser,
                "pam-home" => FileSystemLayoutType.PamHome,
                "pam-home-chroot" => FileSystemLayoutType.PamHomeChroot,
                _ => throw new ArgumentOutOfRangeException(
                    $"Value must be one of \"single-root\", \"root-per-user\", \"pam-home\", \"pam-home-chroot\", but was , \"{value}\"")
                };
        }

        /// <summary>
        /// Gets or sets System.IO-based file system options.
        /// </summary>
        [JsonProperty("system-io")]
        public FileSystemSystemIoOptions SystemIo { get; set; } = new FileSystemSystemIoOptions();

        /// <summary>
        /// Gets or sets Linux API-based file system options.
        /// </summary>
        public FileSystemUnixOptions Unix { get; set; } = new FileSystemUnixOptions();

        /// <summary>
        /// Gets or sets in-memory file system options.
        /// </summary>
        [JsonProperty("in-memory")]
        public FileSystemInMemoryOptions InMemory { get; set; } = new FileSystemInMemoryOptions();

        /// <summary>
        /// Gets or sets Google Drive file system options.
        /// </summary>
        [JsonProperty("google-drive")]
        public FileSystemGoogleDriveOptions GoogleDrive { get; set; } = new FileSystemGoogleDriveOptions();

        internal FileSystemLayoutType LayoutType
        {
            get => _layout ?? FileSystemLayoutType.SingleRoot;
            set => _layout = value;
        }

        internal FileSystemType BackendType
        {
            get => _backend;
            set => _backend = value;
        }
    }
}
