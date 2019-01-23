// <copyright file="IFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// A basic FTP user interface.
    /// </summary>
    public interface IFtpUser
    {
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Returns <c>true</c> when the user is in the given group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <returns><c>true</c> when the user is in the queries <paramref name="groupName"/>.</returns>
        bool IsInGroup([NotNull] string groupName);
    }
}
