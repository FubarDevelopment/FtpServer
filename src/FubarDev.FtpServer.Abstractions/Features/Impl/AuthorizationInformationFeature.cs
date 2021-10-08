// <copyright file="AuthorizationInformationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IAuthorizationInformationFeature"/>.
    /// </summary>
    internal class AuthorizationInformationFeature : IAuthorizationInformationFeature
    {
        /// <inheritdoc />
        [Obsolete("Use the FtpUser property.")]
        public IFtpUser? User { get; set; }

        /// <inheritdoc />
        public ClaimsPrincipal? FtpUser { get; set; }

        /// <inheritdoc />
        public IMembershipProvider? MembershipProvider { get; set; }
    }
}
