// <copyright file="IFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// A basic FTP user interface.
    /// </summary>
    [Obsolete("Use ClaimsPrincipal")]
    public interface IFtpUser
    {
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns <c>true</c> when the user is in the given group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <returns><c>true</c> when the user is in the queries <paramref name="groupName"/>.</returns>
        bool IsInGroup(string groupName);
    }
}
