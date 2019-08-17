// <copyright file="IAccountDirectoryQuery.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Queries directories for a given account.
    /// </summary>
    public interface IAccountDirectoryQuery
    {
        /// <summary>
        /// Get the account directories from the account information.
        /// </summary>
        /// <param name="accountInformation">The account to get the directories from.</param>
        /// <returns>The directories for the account.</returns>
        IAccountDirectories GetDirectories(IAccountInformation accountInformation);
    }
}
