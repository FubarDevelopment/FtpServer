// <copyright file="IFtpCommandBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The base interface for command handlers and extensions.
    /// </summary>
    public interface IFtpCommandBase
    {
        /// <summary>
        /// Gets a collection of all command names for this command.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        [Obsolete("The mapping from name to command handler is created by using the FtpCommandHandlerAttribute.")]
        IReadOnlyCollection<string> Names { get; }

        /// <summary>
        /// Processes the command.
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="cancellationToken">The cancellation token to signal command abortion.</param>
        /// <returns>The FTP response.</returns>
        [NotNull]
        [ItemCanBeNull]
        Task<IFtpResponse> Process([NotNull] FtpCommand command, CancellationToken cancellationToken);
    }
}
