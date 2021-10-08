// <copyright file="IAuthorizationInformationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Authorization information feature.
    /// </summary>
    public interface IAuthorizationInformationFeature
    {
        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        [Obsolete("Use the FtpUser property.")]
        IFtpUser? User { get; set; }

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        ClaimsPrincipal? FtpUser { get; set; }

        /// <summary>
        /// Gets or sets the membership provider that authenticated the <see cref="FtpUser"/>.
        /// </summary>
        IMembershipProvider? MembershipProvider { get; set; }
    }
}
