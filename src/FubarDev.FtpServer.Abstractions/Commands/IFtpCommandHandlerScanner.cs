// <copyright file="IFtpCommandHandlerScanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Searches for FTP command handlers and returns information about it.
    /// </summary>
    public interface IFtpCommandHandlerScanner
    {
        /// <summary>
        /// Search for FTP command handlers.
        /// </summary>
        /// <returns>The information about the found FTP command handlers.</returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IFtpCommandHandlerInformation> Search();
    }
}
