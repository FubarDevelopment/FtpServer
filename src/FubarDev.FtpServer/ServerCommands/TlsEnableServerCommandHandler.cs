// <copyright file="TlsEnableServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.ServerCommands
{
    /// <summary>
    /// Handler for the <see cref="TlsEnableServerCommand"/>.
    /// </summary>
    public class TlsEnableServerCommandHandler : IServerCommandHandler<TlsEnableServerCommand>
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

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
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull] IOptions<AuthTlsOptions> options)
        {
            _connectionAccessor = connectionAccessor;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _serverCertificate = options.Value.ServerCertificate;
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(TlsEnableServerCommand command, CancellationToken cancellationToken)
        {
            var conn = _connectionAccessor.FtpConnection;
            var localizationFeature = conn.Features.Get<ILocalizationFeature>();
            var serverCommandsFeature = conn.Features.Get<IServerCommandFeature>();

            if (_serverCertificate == null)
            {
                var errorMessage = localizationFeature.Catalog.GetString("TLS not configured");
                await serverCommandsFeature.ServerCommandWriter.WriteAsync(
                        new SendResponseServerCommand(new FtpResponse(421, errorMessage)),
                        cancellationToken)
                   .ConfigureAwait(false);
                return;
            }

            var secureConnectionFeature = conn.Features.Get<ISecureConnectionFeature>();
            await secureConnectionFeature.SocketStream.FlushAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var sslStream = await _sslStreamWrapperFactory.WrapStreamAsync(
                        secureConnectionFeature.OriginalStream,
                        true,
                        _serverCertificate,
                        cancellationToken)
                   .ConfigureAwait(false);
                if (secureConnectionFeature.SocketStream != secureConnectionFeature.OriginalStream)
                {
                    // Close old SSL connection.
                    await _sslStreamWrapperFactory.CloseStreamAsync(secureConnectionFeature.SocketStream, cancellationToken)
                       .ConfigureAwait(false);
                }

                secureConnectionFeature.SocketStream = sslStream;
            }
            catch (Exception ex)
            {
                conn.Log?.LogWarning(0, ex, "SSL stream authentication failed: {0}", ex.Message);
                var errorMessage = localizationFeature.Catalog.GetString("TLS authentication failed");
                await serverCommandsFeature.ServerCommandWriter.WriteAsync(
                        new SendResponseServerCommand(new FtpResponse(421, errorMessage)),
                        cancellationToken)
                   .ConfigureAwait(false);
            }
        }
    }
}
