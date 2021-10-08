// <copyright file="IAccountInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Information about the account associated to a connection.
    /// </summary>
    public interface IAccountInformation
    {
        /// <summary>
        /// Gets the current user.
        /// </summary>
        [Obsolete("Use FtpUser to get the user information.")]
        IFtpUser User { get; }

        /// <summary>
        /// Gets the current FTP user.
        /// </summary>
        ClaimsPrincipal FtpUser { get; }

        /// <summary>
        /// Gets the membership provider which authenticated the <see cref="FtpUser"/>.
        /// </summary>
        IMembershipProvider MembershipProvider { get; }
    }
}
