// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace TestFtpServer
{
    /// <summary>
    /// The options for the FTP server.
    /// </summary>
    public class TestFtpServerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the help message should be shown.
        /// </summary>
        public bool ShowHelp { get; set; }

        /// <summary>
        /// Gets or sets the requested server address.
        /// </summary>
        public string? ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the requested FTP server port.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the FTP server should use implicit FTPS.
        /// </summary>
        public bool ImplicitFtps { get; set; }

        /// <summary>
        /// Gets or sets the path to the server certificate file.
        /// </summary>
        public string? ServerCertificateFile { get; set; }

        /// <summary>
        /// Gets or sets the password of the server certificate file.
        /// </summary>
        public string? ServerCertificatePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the in-memory file system should be kept between two connects.
        /// </summary>
        public bool KeepAnonymousInMemoryFileSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Google Drive access token should be refreshed.
        /// </summary>
        public bool RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether background upload should be used.
        /// </summary>
        public bool UseBackgroundUpload { get; set; }

        /// <summary>
        /// Gets or sets the membership provider to be used.
        /// </summary>
        public MembershipProviderType MembershipProviderType { get; set; } = MembershipProviderType.Default;

        /// <summary>
        /// Gets or sets the passive port range.
        /// </summary>
        public (int, int)? PassivePortRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PAM account management is disabled.
        /// </summary>
        public bool NoPamAccountManagement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user ID should be changed during file system operations.
        /// </summary>
        public bool DisableUserIdSwitch { get; set; }

        /// <summary>
        /// Gets or sets the directory layout.
        /// </summary>
        public DirectoryLayout DirectoryLayout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to accept PASV connections from any source.
        /// </summary>
        /// <remarks>
        /// If false (default), connections to a PASV port will only be accepted from the same IP that issued
        /// the respective PASV command.
        /// </remarks>
        public bool PromiscuousPasv { get; set; }

        /// <summary>
        /// Gets the requested or the default port.
        /// </summary>
        /// <returns></returns>
        public int GetPort()
        {
            return Port ?? (ImplicitFtps ? 990 : 21);
        }

        /// <summary>
        /// Validates the current configuration.
        /// </summary>
        public void Validate()
        {
            if (ImplicitFtps && !string.IsNullOrEmpty(ServerCertificateFile))
            {
                throw new Exception("Implicit FTPS requires a server certificate.");
            }
        }
    }

    /// <summary>
    /// The requested directory layout.
    /// </summary>
    public enum DirectoryLayout
    {
        /// <summary>
        /// A single root directory for all users.
        /// </summary>
        SingleRoot,

        /// <summary>
        /// A per-user root directory.
        /// </summary>
        RootPerUser,

        /// <summary>
        /// A single root, but with the users home directory as default directory.
        /// </summary>
        PamHomeDirectory,

        /// <summary>
        /// Users home directories are root.
        /// </summary>
        PamHomeDirectoryAsRoot,
    }

    /// <summary>
    /// The selected membership provider.
    /// </summary>
    [Flags]
    public enum MembershipProviderType
    {
        /// <summary>
        /// Use the default membership provider (<see cref="Anonymous"/>).
        /// </summary>
        Default = 0,

        /// <summary>
        /// Use the custom (example) membership provider.
        /// </summary>
        Custom = 1,

        /// <summary>
        /// Use the membership provider for anonymous users.
        /// </summary>
        Anonymous = 2,

        /// <summary>
        /// Use the PAM membership provider.
        /// </summary>
        PAM = 4,
    }
}
