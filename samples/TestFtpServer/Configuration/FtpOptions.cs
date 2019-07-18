// <copyright file="FtpOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The root object for all options.
    /// </summary>
    public class FtpOptions
    {
        private FileSystemLayoutType? _layout;

        /// <summary>
        /// Gets or sets authentication providers to use.
        /// </summary>
        public MembershipProviderType Authentication { get; set; } = MembershipProviderType.Default;

        /// <summary>
        /// Gets or sets a value indicating whether user/group IDs will be set for file system operations.
        /// </summary>
        public bool SetFileSystemId { get; set; }

        /// <summary>
        /// Gets or sets the bits to be removed from the default file system entry permissions.
        /// </summary>
        public string? Umask { get; set; }

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
            get
            {
                switch (BackendType)
                {
                    case FileSystemType.InMemory: return "in-memory";
                    case FileSystemType.SystemIO: return "system-io";
                    case FileSystemType.Unix: return "unix";
                    case FileSystemType.GoogleDriveUser: return "google-drive:user";
                    case FileSystemType.GoogleDriveService: return "google-drive:service";
                    default: return "in-memory";
                }
            }
            set
            {
                switch (value)
                {
                    case "inMemory":
                    case "in-memory":
                        BackendType = FileSystemType.InMemory;
                        break;
                    case "systemIo":
                    case "system-io":
                        BackendType = FileSystemType.SystemIO;
                        break;
                    case "unix":
                        BackendType = FileSystemType.Unix;
                        break;
                    case "googleDrive:user":
                    case "google-drive:user":
                        BackendType = FileSystemType.GoogleDriveUser;
                        break;
                    case "googleDrive:service":
                    case "google-drive:service":
                        BackendType = FileSystemType.GoogleDriveService;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Value must be one of \"in-memory\", \"system-io\", \"unix\", \"google-drive:user\", \"google-drive:service\", but was , \"{value}\"");
                }
            }
        }

        /// <summary>
        /// Gets or sets the file system layout to use.
        /// </summary>
        public string Layout
        {
            get
            {
                switch (_layout)
                {
                    case FileSystemLayoutType.SingleRoot: return "single-root";
                    case FileSystemLayoutType.RootPerUser: return "root-per-user";
                    case FileSystemLayoutType.PamHome: return "pam-home";
                    case FileSystemLayoutType.PamHomeChroot: return "pam-home-chroot";
                    default: return "single-root";
                }
            }
            set
            {
                switch (value)
                {
                    case "default":
                        _layout = null;
                        break;
                    case "single-root":
                        _layout = FileSystemLayoutType.SingleRoot;
                        break;
                    case "root-per-user":
                        _layout = FileSystemLayoutType.RootPerUser;
                        break;
                    case "pam-home":
                        _layout = FileSystemLayoutType.PamHome;
                        break;
                    case "pam-home-chroot":
                        _layout = FileSystemLayoutType.PamHomeChroot;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Value must be one of \"single-root\", \"root-per-user\", \"pam-home\", \"pam-home-chroot\", but was , \"{value}\"");
                }
            }
        }

        /// <summary>
        /// Gets or sets System.IO-based file system options.
        /// </summary>
        public FileSystemSystemIoOptions SystemIo { get; set; } = new FileSystemSystemIoOptions();

        /// <summary>
        /// Gets or sets Linux API-based file system options.
        /// </summary>
        public FileSystemUnixOptions Unix { get; set; } = new FileSystemUnixOptions();

        /// <summary>
        /// Gets or sets in-memory file system options.
        /// </summary>
        public FileSystemInMemoryOptions InMemory { get; set; } = new FileSystemInMemoryOptions();

        /// <summary>
        /// Gets or sets Google Drive file system options.
        /// </summary>
        public FileSystemGoogleDriveOptions GoogleDrive { get; set; } = new FileSystemGoogleDriveOptions();

        internal FileSystemLayoutType LayoutType
        {
            get => _layout ?? FileSystemLayoutType.SingleRoot;
            set => _layout = value;
        }

        internal FileSystemType BackendType { get; set; } = FileSystemType.InMemory;
    }
}
