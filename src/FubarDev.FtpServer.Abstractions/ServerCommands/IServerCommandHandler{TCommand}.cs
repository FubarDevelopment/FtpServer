// <copyright file="IServerCommandHandler{TCommand}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Interface to be implemented by a server command handler.
    /// </summary>
    /// <typeparam name="TCommand">The server command type.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IServerCommandHandler<TCommand>
        where TCommand : class, IServerCommand
    {
        /// <summary>
        /// Executes the server command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        Task ExecuteAsync([NotNull] TCommand command, CancellationToken cancellationToken);
    }
}
