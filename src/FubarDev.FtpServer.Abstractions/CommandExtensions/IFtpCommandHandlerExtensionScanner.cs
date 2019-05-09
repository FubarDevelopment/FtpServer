// <copyright file="IFtpCommandHandlerExtensionScanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Searches for FTP command handler extensions and returns information about it.
    /// </summary>
    public interface IFtpCommandHandlerExtensionScanner
    {
        /// <summary>
        /// Search for FTP command handler extensions.
        /// </summary>
        /// <returns>The information about the found FTP command handler extensions.</returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IFtpCommandHandlerExtensionInformation> Search();
    }
}
