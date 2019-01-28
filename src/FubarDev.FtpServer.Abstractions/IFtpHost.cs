// <copyright file="IFtpHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;

using JetBrains.Annotations;

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
        [NotNull]
        HostInfo Info { get; }

        /// <summary>
        /// Gets the authentication mechanisms for this host.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IAuthenticationMechanism> AuthenticationMechanisms { get; }

        /// <summary>
        /// Gets the authorization mechanisms for this host.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IAuthorizationMechanism> AuthorizationMechanisms { get; }
    }
}
