// <copyright file="AuthTlsOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Cryptography.X509Certificates;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Options for SSL/TLS connections.
    /// </summary>
    public class AuthTlsOptions
    {
        /// <summary>
        /// Gets or sets the server certificate.
        /// </summary>
        public X509Certificate? ServerCertificate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether implicit FTPS is used.
        /// </summary>
        public bool ImplicitFtps { get; set; }
    }
}
