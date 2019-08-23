// <copyright file="IAnonymousFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// The anonymous FTP interface.
    /// </summary>
    /// <remarks>
    /// The password is interpreted as e-mail.
    /// </remarks>
    [Obsolete("Use ClaimsPrincipal")]
    public interface IAnonymousFtpUser : IFtpUser
    {
        /// <summary>
        /// Gets the e-mail of the anonymous user which was given as password.
        /// </summary>
        string? Email { get; }
    }
}
