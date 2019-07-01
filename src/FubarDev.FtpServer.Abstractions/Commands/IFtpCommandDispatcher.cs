// <copyright file="IFtpCommandDispatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Interface for a FTP command dispatcher.
    /// </summary>
    public interface IFtpCommandDispatcher
    {
        /// <summary>
        /// Passes the FTP commands to the handlers.
        /// </summary>
        /// <param name="context">The context for the FTP command execution.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        Task DispatchAsync([NotNull] FtpContext context, CancellationToken cancellationToken);
    }
}
