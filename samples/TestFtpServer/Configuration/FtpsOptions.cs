// <copyright file="FtpsOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for FTPS.
    /// </summary>
    public class FtpsOptions
    {
        /// <summary>
        /// Gets or sets the path to the X.509 certificate (with or without private key).
        /// </summary>
        /// <remarks>
        /// A private key must be specified if this file isn't a PKCS#12 file with a private key.
        /// </remarks>
        [CanBeNull]
        public string Certificate { get; set; }

        /// <summary>
        /// Gets or sets the path to the private key of the X.509 certificate.
        /// </summary>
        [CanBeNull]
        public string PrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the password for the <see cref="PrivateKey"/> or PKCS#12-formatted <see cref="Certificate"/> which contains the private key.
        /// </summary>
        [CanBeNull]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use implicit AUTO TLS.
        /// </summary>
        public bool Implicit { get; set; }
    }
}
