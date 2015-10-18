//-----------------------------------------------------------------------
// <copyright file="IFtpCommandHandlerFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// A factory to create a command handler for a given connection
    /// </summary>
    public interface IFtpCommandHandlerFactory
    {
        /// <summary>
        /// Create all command handlers for the given <paramref name="connection"/>
        /// </summary>
        /// <param name="connection">The connection to create the command handlers for</param>
        /// <returns>The new <see cref="FtpCommandHandler"/>s</returns>
        [NotNull, ItemNotNull]
        IEnumerable<FtpCommandHandler> CreateCommandHandlers(FtpConnection connection);

        /// <summary>
        /// Create all command handler extensions for the given <paramref name="connection"/>
        /// </summary>
        /// <param name="connection">The connection to create the command handler extensions for</param>
        /// <returns>The new <see cref="FtpCommandHandlerExtension"/>s</returns>
        [NotNull, ItemNotNull]
        IEnumerable<FtpCommandHandlerExtension> CreateCommandHandlerExtensions(FtpConnection connection);
    }
}
