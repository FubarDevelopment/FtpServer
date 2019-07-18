// <copyright file="IFtpHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Information about an FTP host.
    /// </summary>
    public interface IFtpHost
    {
        /// <summary>
        /// Gets the FTP host information as passed to the <c>HOST</c> command.
        /// </summary>
        HostInfo Info { get; }

        /// <summary>
        /// Gets the certificate for this host.
        /// </summary>
        X509Certificate? Certificate { get; }

        /// <summary>
        /// Gets the authentication mechanisms for this host.
        /// </summary>
        IEnumerable<IAuthenticationMechanism> AuthenticationMechanisms { get; }

        /// <summary>
        /// Gets the authorization mechanisms for this host.
        /// </summary>
        IEnumerable<IAuthorizationMechanism> AuthorizationMechanisms { get; }
    }
}
