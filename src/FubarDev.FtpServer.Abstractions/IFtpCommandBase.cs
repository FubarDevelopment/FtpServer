// <copyright file="IFtpCommandBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The base interface for command handlers and extensions.
    /// </summary>
    public interface IFtpCommandBase
    {
        /// <summary>
        /// Processes the command.
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="cancellationToken">The cancellation token to signal command abortion.</param>
        /// <returns>The FTP response.</returns>
        Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken);
    }
}
