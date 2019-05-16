// <copyright file="IServerCommandExecutor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// A server command executor.
    /// </summary>
    public interface IServerCommandExecutor
    {
        /// <summary>
        /// Execute the given server command.
        /// </summary>
        /// <param name="serverCommand">The server command to execute.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        Task ExecuteAsync(
            [NotNull] IServerCommand serverCommand,
            CancellationToken cancellationToken);
    }
}
