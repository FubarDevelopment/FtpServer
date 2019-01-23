// <copyright file="IAccountInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.AccountManagement;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Information about the account associated to a connection.
    /// </summary>
    public interface IAccountInformation
    {
        /// <summary>
        /// Gets the current user name.
        /// </summary>
        [NotNull]
        IFtpUser User { get; }

        /// <summary>
        /// Gets the membership provider that was used to authenticate the user.
        /// </summary>
        [NotNull]
        IMembershipProvider AuthenticatedBy { get; }
    }
}
