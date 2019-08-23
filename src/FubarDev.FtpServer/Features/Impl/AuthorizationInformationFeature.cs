// <copyright file="AuthorizationInformationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Compatibility;

using Microsoft.AspNetCore.Connections.Features;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IAuthorizationInformationFeature"/>.
    /// </summary>
    internal class AuthorizationInformationFeature :
#pragma warning disable 618
        IAuthorizationInformationFeature,
#pragma warning restore 618
        IConnectionUserFeature
    {
        /// <inheritdoc />
        [Obsolete("Use the FtpUser property.")]
        public IFtpUser? User { get; set; }

        /// <inheritdoc />
        public ClaimsPrincipal? FtpUser { get; set; }

        /// <inheritdoc />
        ClaimsPrincipal? IConnectionUserFeature.User
        {
            get => FtpUser;
            set
            {
                FtpUser = value;
#pragma warning disable 618
                User = value?.CreateUser();
#pragma warning restore 618
            }
        }
    }
}
