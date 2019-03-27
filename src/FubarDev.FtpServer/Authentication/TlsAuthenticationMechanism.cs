// <copyright file="TlsAuthenticationMechanism.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.Authentication
{
    /// <summary>
    /// Implementation for the <c>AUTH TLS</c> command.
    /// </summary>
    public class TlsAuthenticationMechanism : AuthenticationMechanism, IFeatureHost
    {
        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        [CanBeNull]
        private readonly X509Certificate2 _serverCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TlsAuthenticationMechanism"/> class.
        /// </summary>
        /// <param name="connection">The required FTP connection.</param>
        /// <param name="sslStreamWrapperFactory">The SslStream wrapper factory.</param>
        /// <param name="options">Options for the AUTH TLS command.</param>
        public TlsAuthenticationMechanism(
            [NotNull] IFtpConnection connection,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull] IOptions<AuthTlsOptions> options)
            : base(connection)
        {
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _serverCertificate = options.Value.ServerCertificate;
        }

        /// <inheritdoc />
        public override void Reset()
        {
        }

        /// <inheritdoc />
        public override bool CanHandle(string methodIdentifier)
        {
            return string.Equals(methodIdentifier.Trim(), "TLS", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> HandleAuthAsync(string methodIdentifier, CancellationToken cancellationToken)
        {
            if (_serverCertificate == null)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(502, T("TLS not configured")));
            }

            var response = new FtpResponse(234, T("Enabling TLS Connection"))
            {
                AfterWriteAction = async (conn, ct) =>
                {
                    await conn.SocketStream.FlushAsync(ct).ConfigureAwait(false);

                    try
                    {
                        var sslStream = await _sslStreamWrapperFactory.WrapStreamAsync(
                                conn.OriginalStream,
                                true,
                                _serverCertificate,
                                cancellationToken)
                           .ConfigureAwait(false);
                        if (conn.SocketStream != conn.OriginalStream)
                        {
                            // Close old SSL connection.
                            await _sslStreamWrapperFactory.CloseStreamAsync(conn.SocketStream, cancellationToken)
                               .ConfigureAwait(false);
                        }

                        conn.SocketStream = sslStream;
                    }
                    catch (Exception ex)
                    {
                        conn.Log?.LogWarning(0, ex, "SSL stream authentication failed: {0}", ex.Message);
                        await conn
                           .WriteAsync(new FtpResponse(421, T("TLS authentication failed")), ct)
                           .ConfigureAwait(false);
                    }
                },
            };

            return Task.FromResult<IFtpResponse>(response);
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> HandleAdatAsync(byte[] data, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse>(new FtpResponse(421, T("Service not available")));
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> HandlePbszAsync(long size, CancellationToken cancellationToken)
        {
            if (size != 0)
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("A protection buffer size other than 0 is not supported. Use PBSZ=0 instead.")));
            }

            return Task.FromResult<IFtpResponse>(new FtpResponse(200, T("Protection buffer size set to {0}.", size)));
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> HandleProtAsync(string protCode, CancellationToken cancellationToken)
        {
            switch (protCode.ToUpperInvariant())
            {
                case "C":
                    Connection.Data.CreateEncryptedStream = null;
                    break;
                case "P":
                    Connection.Data.CreateEncryptedStream = CreateSslStream;
                    break;
                default:
                    return Task.FromResult<IFtpResponse>(new FtpResponse(SecurityActionResult.RequestedProtLevelNotSupported, T("A data channel protection level other than C, or P is not supported.")));
            }

            return Task.FromResult<IFtpResponse>(new FtpResponse(200, T("Data channel protection level set to {0}.", protCode)));
        }

        /// <inheritdoc />
        public IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            if (_serverCertificate != null)
            {
                yield return new GenericFeatureInfo("AUTH", conn => "AUTH TLS", false);
                yield return new GenericFeatureInfo("PBSZ", false);
                yield return new GenericFeatureInfo("PROT", false);
            }
        }

        private async Task<Stream> CreateSslStream(Stream unencryptedStream)
        {
            var sslStream = await _sslStreamWrapperFactory.WrapStreamAsync(
                    unencryptedStream,
                    false,
                    _serverCertificate,
                    CancellationToken.None)
               .ConfigureAwait(false);
            return sslStream;
        }
    }
}
