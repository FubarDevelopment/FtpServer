// <copyright file="ResumeConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for the <see cref="ResumeConnectionServerCommand"/>.
    /// </summary>
    public class ResumeConnectionServerCommandHandler : IServerCommandHandler<ResumeConnectionServerCommand>
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;

        private readonly ILogger<ResumeConnectionServerCommandHandler>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResumeConnectionServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="logger">The logger.</param>
        public ResumeConnectionServerCommandHandler(
            IFtpConnectionAccessor connectionAccessor,
            ILogger<ResumeConnectionServerCommandHandler>? logger = null)
        {
            _connectionAccessor = connectionAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(ResumeConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;
            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();

            await networkStreamFeature.SecureConnectionAdapter.Receiver.ContinueAsync(cancellationToken)
               .ConfigureAwait(false);
            _logger?.LogDebug("Receiver resumed");
        }
    }
}
