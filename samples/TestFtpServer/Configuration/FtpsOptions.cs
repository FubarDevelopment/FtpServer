// <copyright file="FtpsOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for FTPS.
    /// </summary>
    public class FtpsOptions
    {
        /// <summary>
        /// Gets or sets the path to the X.509 certificate (with private key).
        /// </summary>
        public string? Certificate { get; set; }

        /// <summary>
        /// Gets or sets the password for the <see cref="Certificate"/>.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use implicit AUTO TLS.
        /// </summary>
        public bool Implicit { get; set; }
    }
}
