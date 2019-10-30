// <copyright file="AsyncDataConnectionDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Delegate for sending or receivung data over a data connection.
    /// </summary>
    /// <param name="dataConnection">The data connection used to send or receive the data over.</param>
    /// <param name="cancellationToken">The cancellation token to signal command abortion.</param>
    /// <returns>The task with an FTP response if it should be different than the default one.</returns>
    public delegate Task<IFtpResponse?> AsyncDataConnectionDelegate(IFtpDataConnection dataConnection, CancellationToken cancellationToken);
}
