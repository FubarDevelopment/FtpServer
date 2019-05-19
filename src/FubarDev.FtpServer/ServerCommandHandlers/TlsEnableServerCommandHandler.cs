// <copyright file="TlsEnableServerCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
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

        /// <summary>
        /// Enables TLS on a connection that isn't reading or writing (read: that's not started yet or is paused).
        /// </summary>
        /// <param name="connection">The FTP connection to activate TLS for.</param>
        /// <param name="certificate">The X.509 certificate to use (with private key).</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task EnableTlsAsync(
            [NotNull] IFtpConnection connection,
            [NotNull] X509Certificate2 certificate,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            CancellationToken cancellationToken)
        {
            var localizationFeature = connection.Features.Get<ILocalizationFeature>();
            var serverCommandsFeature = connection.Features.Get<IServerCommandFeature>();

            var networkStreamFeature = connection.Features.Get<INetworkStreamFeature>();

            var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();
            await secureConnectionFeature.SocketStream.FlushAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await secureConnectionFeature.CloseEncryptedControlStream(
                        secureConnectionFeature.SocketStream,
                        cancellationToken)
                   .ConfigureAwait(false);

                var sslStream = await sslStreamWrapperFactory.WrapStreamAsync(
                        secureConnectionFeature.OriginalStream,
                        true,
                        certificate,
                        cancellationToken)
                   .ConfigureAwait(false);

                secureConnectionFeature.CloseEncryptedControlStream =
                    (encryptedStream, ct) => CloseEncryptedControlConnectionAsync(
                        networkStreamFeature,
                        secureConnectionFeature,
                        sslStreamWrapperFactory,
                        ct);

                secureConnectionFeature.SocketStream = sslStream;
                networkStreamFeature.StreamReaderService.Stream = sslStream;
                networkStreamFeature.StreamWriterService.Stream = sslStream;
            }
            catch (Exception ex)
            {
                connection.Log?.LogWarning(0, ex, "SSL stream authentication failed: {0}", ex.Message);
                var errorMessage = localizationFeature.Catalog.GetString("TLS authentication failed");
                await serverCommandsFeature.ServerCommandWriter.WriteAsync(
                        new SendResponseServerCommand(new FtpResponse(421, errorMessage)),
                        cancellationToken)
                   .ConfigureAwait(false);
            }
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
                await EnableTlsAsync(connection, _serverCertificate, _sslStreamWrapperFactory, cancellationToken)
                   .ConfigureAwait(false);
            }
            finally
            {
                await networkStreamFeature.StreamWriterService.ContinueAsync(cancellationToken)
                   .ConfigureAwait(false);
            }
        }

        private static async Task<Stream> CloseEncryptedControlConnectionAsync(
            [NotNull] INetworkStreamFeature networkStreamFeature,
            [NotNull] ISecureConnectionFeature secureConnectionFeature,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            CancellationToken cancellationToken)
        {
            var originalStream = secureConnectionFeature.OriginalStream;

            if (!secureConnectionFeature.IsSecure)
            {
                return originalStream;
            }

            var encryptedStream = secureConnectionFeature.SocketStream;

            var readerDisposeFunc = await networkStreamFeature.StreamReaderService.WrapPauseAsync(cancellationToken)
               .ConfigureAwait(false);
            try
            {
                var writerDisposeFunc = await networkStreamFeature.StreamWriterService.WrapPauseAsync(cancellationToken)
                   .ConfigureAwait(false);
                try
                {
                    await sslStreamWrapperFactory.CloseStreamAsync(encryptedStream, cancellationToken)
                       .ConfigureAwait(false);

                    secureConnectionFeature.CloseEncryptedControlStream = (_, __) => Task.FromResult(originalStream);
                    secureConnectionFeature.SocketStream = originalStream;
                    networkStreamFeature.StreamReaderService.Stream = originalStream;
                    networkStreamFeature.StreamWriterService.Stream = originalStream;
                }
                finally
                {
                    await writerDisposeFunc()
                       .ConfigureAwait(false);
                }
            }
            finally
            {
                await readerDisposeFunc();
            }

            return secureConnectionFeature.OriginalStream;
        }
    }
}
