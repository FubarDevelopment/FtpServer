// <copyright file="SecureDataConnectionWrapper.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.DataConnection
{
    public class SecureDataConnectionWrapper
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        public SecureDataConnectionWrapper(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory)
        {
            _connectionAccessor = connectionAccessor;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
        }

        [NotNull]
        [ItemNotNull]
        public async Task<IFtpDataConnection> WrapAsync([NotNull] IFtpDataConnection dataConnection)
        {
            var connection = _connectionAccessor.FtpConnection;
            var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();

            if (secureConnectionFeature.CreateEncryptedStream == null)
            {
                return dataConnection;
            }

            var newStream = await secureConnectionFeature.CreateEncryptedStream(dataConnection.Stream)
               .ConfigureAwait(false);
            return new SecureFtpDataConnection(dataConnection, _sslStreamWrapperFactory, newStream);
        }

        private class SecureFtpDataConnection : IFtpDataConnection
        {
            [NotNull]
            private readonly IFtpDataConnection _originalDataConnection;

            [NotNull]
            private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

            private bool _closed;

            public SecureFtpDataConnection(
                [NotNull] IFtpDataConnection originalDataConnection,
                [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
                [NotNull] Stream stream)
            {
                _originalDataConnection = originalDataConnection;
                _sslStreamWrapperFactory = sslStreamWrapperFactory;
                LocalAddress = originalDataConnection.LocalAddress;
                RemoteAddress = originalDataConnection.RemoteAddress;
                Stream = stream;
            }

            /// <inheritdoc />
            public IPEndPoint LocalAddress { get; }

            /// <inheritdoc />
            public IPEndPoint RemoteAddress { get; }

            /// <inheritdoc />
            public Stream Stream { get; }

            /// <inheritdoc />
            public async Task CloseAsync(CancellationToken cancellationToken)
            {
                if (_closed)
                {
                    return;
                }

                _closed = true;
                await _sslStreamWrapperFactory.CloseStreamAsync(Stream, cancellationToken)
                   .ConfigureAwait(false);
                await _originalDataConnection.CloseAsync(cancellationToken)
                   .ConfigureAwait(false);
            }
        }
    }
}
