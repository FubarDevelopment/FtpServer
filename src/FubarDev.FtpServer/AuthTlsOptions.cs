// <copyright file="AuthTlsOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Cryptography.X509Certificates;

using JetBrains.Annotations;

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
        [CanBeNull]
        public X509Certificate2 ServerCertificate { get; set; }
    }
}
