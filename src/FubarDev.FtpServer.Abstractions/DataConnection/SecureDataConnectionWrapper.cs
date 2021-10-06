// <copyright file="SecureDataConnectionWrapper.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.DataConnection
{
    /// <summary>
    /// Wrapper that wraps a data connection into a secure data connection if needed.
    /// </summary>
    public class SecureDataConnectionWrapper
    {
        private readonly IFtpConnectionAccessor _connectionAccessor;
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureDataConnectionWrapper"/> class.
        /// </summary>
        /// <param name="connectionAccessor">Accessor for the FTP connection.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        public SecureDataConnectionWrapper(
            IFtpConnectionAccessor connectionAccessor,
            ISslStreamWrapperFactory sslStreamWrapperFactory)
        {
            _connectionAccessor = connectionAccessor;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
        }

        /// <summary>
        /// Wraps the data connection into a secure data connection if needed.
        /// </summary>
        /// <param name="dataConnection">The data connection that should - if needed - be wrapped into a secure data connection.</param>
        /// <returns>The task returning the same or a secure data connection.</returns>
        public async Task<IFtpDataConnection> WrapAsync(IFtpDataConnection dataConnection)
        {
            var connection = _connectionAccessor.FtpConnection;
            var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();
            var newStream = await secureConnectionFeature.CreateEncryptedStream(dataConnection.Stream)
               .ConfigureAwait(false);

            return ReferenceEquals(newStream, dataConnection.Stream)
                ? dataConnection
                : new SecureFtpDataConnection(dataConnection, _sslStreamWrapperFactory, newStream);
        }

        private class SecureFtpDataConnection : IFtpDataConnection
        {
            private readonly IFtpDataConnection _originalDataConnection;
            private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

            private bool _closed;

            public SecureFtpDataConnection(
                IFtpDataConnection originalDataConnection,
                ISslStreamWrapperFactory sslStreamWrapperFactory,
                Stream stream)
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
            public bool Closed => _closed;

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
