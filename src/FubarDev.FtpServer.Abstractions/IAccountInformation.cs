// <copyright file="IAccountInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Information about the account associated to a connection.
    /// </summary>
    public interface IAccountInformation
    {
        /// <summary>
        /// Gets the current FTP user.
        /// </summary>
        ClaimsPrincipal FtpUser { get; }
    }
}
