// <copyright file="CloseConnectionServerCommandHandler.cs" company="Fubar Development Junker">
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
    /// Handler for the <see cref="CloseConnectionServerCommand"/>.
    /// </summary>
    public class CloseConnectionServerCommandHandler : IServerCommandHandler<CloseConnectionServerCommand>
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseConnectionServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        public CloseConnectionServerCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor)
        {
            _connectionAccessor = connectionAccessor;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(CloseConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;

            // Pause the sender (which also flushes the remaining data)
            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();
            await networkStreamFeature.StreamWriterService.PauseAsync(cancellationToken)
               .ConfigureAwait(false);

            // Flush the stream
            var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();
            var socketStream = secureConnectionFeature.SocketStream;
            await socketStream.FlushAsync(cancellationToken)
               .ConfigureAwait(false);

            // Close the SSL stream.
            await secureConnectionFeature.CloseEncryptedControlStream(
                    secureConnectionFeature.SocketStream,
                    cancellationToken)
               .ConfigureAwait(false);

            await Task.WhenAny(connection.StopAsync(), Task.Delay(-1, cancellationToken))
               .ConfigureAwait(false);
        }
    }
}
