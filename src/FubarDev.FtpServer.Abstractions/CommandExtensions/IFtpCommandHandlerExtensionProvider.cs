// <copyright file="IFtpCommandHandlerExtensionProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Provides information about all found FTP command handler extensions.
    /// </summary>
    public interface IFtpCommandHandlerExtensionProvider
    {
        /// <summary>
        /// Gets the information for all command handler extensions.
        /// </summary>
        IEnumerable<IFtpCommandHandlerExtensionInformation> CommandHandlerExtensions { get; }
    }
}
