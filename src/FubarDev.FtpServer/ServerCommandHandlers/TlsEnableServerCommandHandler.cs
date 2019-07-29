// <copyright file="TlsEnableServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.ServerCommandHandlers
{
    /// <summary>
    /// Handler for the <see cref="TlsEnableServerCommand"/>.
    /// </summary>
    public class TlsEnableServerCommandHandler : IServerCommandHandler<TlsEnableServerCommand>
    {
        private readonly IFtpConnectionContextAccessor _connectionContextAccessor;
        private readonly ILogger<TlsEnableServerCommandHandler>? _logger;
        private readonly X509Certificate2? _serverCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TlsEnableServerCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionContextAccessor">The FTP connection accessor.</param>
        /// <param name="options">Options for the AUTH TLS command.</param>
        /// <param name="logger">The logger.</param>
        public TlsEnableServerCommandHandler(
            IFtpConnectionContextAccessor connectionContextAccessor,
            IOptions<AuthTlsOptions> options,
            ILogger<TlsEnableServerCommandHandler>? logger = null)
        {
            _connectionContextAccessor = connectionContextAccessor;
            _logger = logger;
            _serverCertificate = options.Value.ServerCertificate;
        }

        /// <summary>
        /// Enables TLS on a connection that isn't reading or writing (read: that's not started yet or is paused).
        /// </summary>
        /// <param name="connectionContext">The FTP connection context to activate TLS for.</param>
        /// <param name="certificate">The X.509 certificate to use (with private key).</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task EnableTlsAsync(
            IFtpConnectionContext connectionContext,
            X509Certificate2 certificate,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            var networkStreamFeature = connectionContext.Features.Get<INetworkStreamFeature>();
            var service = networkStreamFeature.SecureConnectionAdapter;

            var secureConnectionFeature = connectionContext.Features.Get<ISecureConnectionFeature>();
            logger?.LogTrace("Enable SslStream");
            await service.EnableSslStreamAsync(certificate, cancellationToken)
               .ConfigureAwait(false);

            logger?.LogTrace("Set close function");
            secureConnectionFeature.CloseEncryptedControlStream =
                ct => CloseEncryptedControlConnectionAsync(
                    networkStreamFeature,
                    secureConnectionFeature,
                    ct);
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(TlsEnableServerCommand command, CancellationToken cancellationToken)
        {
            var connection = _connectionContextAccessor.FtpConnectionContext;
            var serverCommandsFeature = connection.Features.Get<IServerCommandFeature>();
            var localizationFeature = connection.Features.Get<ILocalizationFeature>();

            if (_serverCertificate == null)
            {
                var errorMessage = localizationFeature.Catalog.GetString("TLS not configured");
                await serverCommandsFeature.ServerCommandWriter.WriteAsync(
                        new SendResponseServerCommand(new FtpResponse(421, errorMessage)),
                        cancellationToken)
                   .ConfigureAwait(false);
                return;
            }

            try
            {
                await EnableTlsAsync(connection, _serverCertificate, _logger, cancellationToken)
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorMessage = localizationFeature.Catalog.GetString("TLS negotiation error: {0}", ex.Message);
                await serverCommandsFeature.ServerCommandWriter.WriteAsync(
                        new SendResponseServerCommand(new FtpResponse(421, errorMessage)),
                        cancellationToken)
                   .ConfigureAwait(false);
            }
        }

        private static async Task CloseEncryptedControlConnectionAsync(
            INetworkStreamFeature networkStreamFeature,
            ISecureConnectionFeature secureConnectionFeature,
            CancellationToken cancellationToken)
        {
            var service = networkStreamFeature.SecureConnectionAdapter;
            await service.ResetAsync(cancellationToken).ConfigureAwait(false);

            secureConnectionFeature.CreateEncryptedStream = Task.FromResult;
            secureConnectionFeature.CloseEncryptedControlStream = ct => Task.CompletedTask;
        }
    }
}
