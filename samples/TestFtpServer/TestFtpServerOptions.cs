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
        public bool ShowHelp { get;set; }

        /// <summary>
        /// Gets or sets the requested server address.
        /// </summary>
        public string ServerAddress { get; set; }

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
        public string ServerCertificateFile { get; set; }

        /// <summary>
        /// Gets or sets the password of the server certificate file.
        /// </summary>
        public string ServerCertificatePassword { get; set; }

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
        public MembershipProviderType MembershipProviderType { get; set; } = MembershipProviderType.Anonymous;

        /// <summary>
        /// Gets or sets the passive port range.
        /// </summary>
        public (int, int)? PassivePortRange { get; set; }

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
    /// The selected membership provider.
    /// </summary>
    public enum MembershipProviderType
    {
        /// <summary>
        /// Use the custom (example) membership provider.
        /// </summary>
        Custom,

        /// <summary>
        /// Use the membership provider for anonymous users.
        /// </summary>
        Anonymous,
    }
}
