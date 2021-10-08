// <copyright file="CloseConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

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
        public async Task ExecuteAsync(CloseConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;

            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();
            await networkStreamFeature.SecureConnectionAdapter.StopAsync(cancellationToken);

            // Just abort the connection. This should avoid problems with an ObjectDisposedException.
            // The "StopAsync" will be called in CommandChannelDispatcherAsync.
            ((FtpConnection)connection).Abort();
        }
    }
}
