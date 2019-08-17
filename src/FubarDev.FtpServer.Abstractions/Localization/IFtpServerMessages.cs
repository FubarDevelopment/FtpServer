// <copyright file="IFtpServerMessages.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.Localization
{
    /// <summary>
    /// Interface to get the default messages.
    /// </summary>
    /// <remarks>
    /// This is incomplete yet, but it supports (at least)
    /// the configuration of banner and greeting messages.
    /// </remarks>
    public interface IFtpServerMessages
    {
        /// <summary>
        /// Gets the FTP servers banner message.
        /// </summary>
        /// <returns>the FTP servers banner message.</returns>
        IEnumerable<string> GetBannerMessage();

        /// <summary>
        /// Gets the message that the directory could be changed successfully.
        /// </summary>
        /// <param name="path">The path the message needs to be created for.</param>
        /// <returns>the FTP servers message.</returns>
        IEnumerable<string> GetDirectoryChangedMessage(
            Stack<IUnixDirectoryEntry> path);

        /// <summary>
        /// Gets the message that the password authorization was successfully.
        /// </summary>
        /// <param name="accountInformation">The account information.</param>
        /// <returns>the FTP servers message.</returns>
        IEnumerable<string> GetPasswordAuthorizationSuccessfulMessage(
            IAccountInformation accountInformation);
    }
}
