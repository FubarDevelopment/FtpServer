// <copyright file="FlushControlConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.ServerCommands;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler implementation for <see cref="FlushControlConnectionServerCommand"/>.
    /// </summary>
    public class FlushControlConnectionServerCommandHandler : IServerCommandHandler<FlushControlConnectionServerCommand>
    {
        /// <inheritdoc />
        public Task ExecuteAsync(FlushControlConnectionServerCommand command, CancellationToken cancellationToken)
        {
            // TODO: Implement!
            return Task.CompletedTask;
        }
    }
}
