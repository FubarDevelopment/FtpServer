// <copyright file="TlsEnableServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.ConnectionHandlers;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Utilities;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for the <see cref="TlsEnableServerCommand"/>.
    /// </summary>
    public class TlsEnableServerCommandHandler : IServerCommandHandler<TlsEnableServerCommand>
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        [CanBeNull]
        private readonly X509Certificate2 _serverCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TlsEnableServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="sslStreamWrapperFactory">The SslStream wrapper factory.</param>
        /// <param name="options">Options for the AUTH TLS command.</param>
        public TlsEnableServerCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] IOptions<AuthTlsOptions> options)
        {
            _connectionAccessor = connectionAccessor;
            _serverCertificate = options.Value.ServerCertificate;
        }

        /// <summary>
        /// Enables TLS on a connection that isn't reading or writing (read: that's not started yet or is paused).
        /// </summary>
        /// <param name="connection">The FTP connection to activate TLS for.</param>
        /// <param name="certificate">The X.509 certificate to use (with private key).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task EnableTlsAsync(
            [NotNull] IFtpConnection connection,
            [NotNull] X509Certificate2 certificate,
            CancellationToken cancellationToken)
        {
            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();
            var service = (TlsStreamService)networkStreamFeature.TlsStreamService;

            var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();
            connection.Log?.LogTrace("Enable SslStream");
            service.EnableSslStream = true;

            connection.Log?.LogTrace("Set close function");
            secureConnectionFeature.CloseEncryptedControlStream =
                ct => CloseEncryptedControlConnectionAsync(
                    networkStreamFeature,
                    secureConnectionFeature,
                    ct);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(TlsEnableServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;

            if (_serverCertificate == null)
            {
                var localizationFeature = connection.Features.Get<ILocalizationFeature>();
                var serverCommandsFeature = connection.Features.Get<IServerCommandFeature>();
                var errorMessage = localizationFeature.Catalog.GetString("TLS not configured");
                await serverCommandsFeature.ServerCommandWriter.WriteAsync(
                        new SendResponseServerCommand(new FtpResponse(421, errorMessage)),
                        cancellationToken)
                   .ConfigureAwait(false);
                return;
            }

            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();

            await networkStreamFeature.StreamWriterService.PauseAsync(cancellationToken)
               .ConfigureAwait(false);

            try
            {
                await EnableTlsAsync(connection, _serverCertificate, cancellationToken)
                   .ConfigureAwait(false);
            }
            finally
            {
                await networkStreamFeature.StreamWriterService.ContinueAsync(cancellationToken)
                   .ConfigureAwait(false);
            }
        }

        private static async Task CloseEncryptedControlConnectionAsync(
            [NotNull] INetworkStreamFeature networkStreamFeature,
            [NotNull] ISecureConnectionFeature secureConnectionFeature,
            CancellationToken cancellationToken)
        {
            var originalStream = secureConnectionFeature.OriginalStream;
            var readerDisposeFunc = await networkStreamFeature.TlsStreamService.WrapPauseAsync(cancellationToken)
               .ConfigureAwait(false);
            try
            {
                var service = (TlsStreamService)networkStreamFeature.TlsStreamService;
                service.EnableSslStream = false;

                secureConnectionFeature.CreateEncryptedStream = null;
                secureConnectionFeature.CloseEncryptedControlStream = ct => Task.CompletedTask;
            }
            finally
            {
                await readerDisposeFunc().ConfigureAwait(false);
            }
        }
    }
}
