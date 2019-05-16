// <copyright file="PauseConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for the <see cref="PauseConnectionServerCommand"/>.
    /// </summary>
    public class PauseConnectionServerCommandHandler : IServerCommandHandler<PauseConnectionServerCommand>
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PauseConnectionServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        public PauseConnectionServerCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor)
        {
            _connectionAccessor = connectionAccessor;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(PauseConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;
            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();

            await networkStreamFeature.StreamReaderService.PauseAsync(cancellationToken)
               .ConfigureAwait(false);
        }
    }
}
