// <copyright file="TlsAuthenticationOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Cryptography.X509Certificates;

namespace FubarDev.FtpServer.Authentication
{
    /// <summary>
    /// Options for SSL/TLS connections.
    /// </summary>
    public class TlsAuthenticationOptions
    {
        /// <summary>
        /// Gets or sets the server certificate.
        /// </summary>
        public X509Certificate2 ServerCertificate { get; set; }
    }
}
