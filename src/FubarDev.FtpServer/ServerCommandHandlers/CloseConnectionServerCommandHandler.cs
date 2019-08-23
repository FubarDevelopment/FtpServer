// <copyright file="CloseConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.ServerCommands;

using Microsoft.AspNetCore.Connections.Features;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for the <see cref="CloseConnectionServerCommand"/>.
    /// </summary>
    public class CloseConnectionServerCommandHandler : IServerCommandHandler<CloseConnectionServerCommand>
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseConnectionServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        public CloseConnectionServerCommandHandler(
            IFtpConnectionAccessor connectionAccessor)
        {
            _connectionAccessor = connectionAccessor;
        }

        /// <inheritdoc />
        public Task ExecuteAsync(CloseConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;

            var lifetimeFeature = connection.Features.Get<IConnectionLifetimeFeature>();
            lifetimeFeature.Abort();

            return Task.CompletedTask;
        }
    }
}
