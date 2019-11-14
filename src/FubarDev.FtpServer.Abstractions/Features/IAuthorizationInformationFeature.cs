// <copyright file="IAuthorizationInformationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

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
        ClaimsPrincipal? FtpUser { get; set; }
    }
}
