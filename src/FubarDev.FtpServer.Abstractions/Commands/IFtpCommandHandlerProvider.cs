// <copyright file="IFtpCommandHandlerProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Provides information about all found FTP command handlers.
    /// </summary>
    public interface IFtpCommandHandlerProvider
    {
        /// <summary>
        /// Gets the information for all command handlers.
        /// </summary>
        IEnumerable<IFtpCommandHandlerInformation> CommandHandlers { get; }
    }
}
