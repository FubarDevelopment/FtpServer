// <copyright file="FtpCommandHandlerProcessDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Processes the command.
    /// </summary>
    /// <param name="command">The command to process.</param>
    /// <param name="cancellationToken">The cancellation token to signal command abortion.</param>
    /// <returns>The FTP response.</returns>
    [NotNull]
    [ItemCanBeNull]
    public delegate Task<IFtpResponse> FtpCommandHandlerProcessDelegate([NotNull] FtpCommand command, CancellationToken cancellationToken);
}
