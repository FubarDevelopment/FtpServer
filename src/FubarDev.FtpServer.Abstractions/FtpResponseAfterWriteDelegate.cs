// <copyright file="FtpResponseAfterWriteDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Delegate to be called after a response was written.
    /// </summary>
    /// <param name="connection">The FTP connection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    [Obsolete("Use a custom server command.")]
    public delegate Task<IFtpResponse?> FtpResponseAfterWriteAsyncDelegate(
        IFtpConnection connection,
        CancellationToken cancellationToken);
}
