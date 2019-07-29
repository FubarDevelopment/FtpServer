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
        private readonly IFtpConnectionContextAccessor _connectionContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseConnectionServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionContextAccessor">The FTP connection accessor.</param>
        public CloseConnectionServerCommandHandler(
            IFtpConnectionContextAccessor connectionContextAccessor)
        {
            _connectionContextAccessor = connectionContextAccessor;
        }

        /// <inheritdoc />
        public Task ExecuteAsync(CloseConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionContextAccessor.FtpConnectionContext;

            // TODO: Evaluate if this **really** works as expected.
            var lifetimeFeature = connection.Features.Get<IConnectionLifetimeFeature>();
            lifetimeFeature.Abort();
            return Task.CompletedTask;

            /*
            // - Flush the remaining data
            // - Close the SslStream (if active)
            // - Stop all connection tasks
            await Task.WhenAny(connection.StopAsync(), Task.Delay(-1, cancellationToken))
               .ConfigureAwait(false);
            */
        }
    }
}
