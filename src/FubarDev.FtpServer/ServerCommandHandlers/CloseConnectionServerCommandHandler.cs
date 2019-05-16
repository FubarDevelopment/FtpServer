// <copyright file="CloseConnectionServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
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

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseConnectionServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        public CloseConnectionServerCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory)
        {
            _connectionAccessor = connectionAccessor;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(CloseConnectionServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;
            var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();
            var socketStream = secureConnectionFeature.SocketStream;
            await socketStream.FlushAsync(cancellationToken)
               .ConfigureAwait(false);

            // Close the SSL stream.
            await secureConnectionFeature.CloseEncryptedControlStream(
                    secureConnectionFeature.SocketStream,
                    cancellationToken)
               .ConfigureAwait(false);

            await connection.StopAsync();
        }
    }
}
