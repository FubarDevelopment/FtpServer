// <copyright file="CloseDataConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.ServerCommands;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for the <see cref="CloseDataConnectionServerCommand"/>.
    /// </summary>
    public class CloseDataConnectionServerCommandHandler : IServerCommandHandler<CloseDataConnectionServerCommand>
    {
        /// <inheritdoc />
        public Task ExecuteAsync(CloseDataConnectionServerCommand command, CancellationToken cancellationToken)
        {
            return command.DataConnection.CloseAsync(cancellationToken);
        }
    }
}
