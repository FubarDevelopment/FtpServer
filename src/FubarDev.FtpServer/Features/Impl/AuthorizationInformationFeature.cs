// <copyright file="AuthorizationInformationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IAuthorizationInformationFeature"/>.
    /// </summary>
    internal class AuthorizationInformationFeature : IAuthorizationInformationFeature
    {
        /// <inheritdoc />
        public ClaimsPrincipal? FtpUser { get; set; }
    }
}
