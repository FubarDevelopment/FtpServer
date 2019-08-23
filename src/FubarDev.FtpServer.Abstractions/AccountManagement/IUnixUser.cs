// <copyright file="IUnixUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Interface for unix user specific information.
    /// </summary>
    [Obsolete("Use ClaimsPrincipal")]
    public interface IUnixUser : IFtpUser
    {
        /// <summary>
        /// Gets the home path.
        /// </summary>
        string? HomePath { get; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        long UserId { get; }

        /// <summary>
        /// Gets the group identifier.
        /// </summary>
        long GroupId { get; }
    }
}
