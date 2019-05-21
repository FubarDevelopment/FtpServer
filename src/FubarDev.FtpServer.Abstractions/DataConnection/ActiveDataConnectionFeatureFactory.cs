// <copyright file="ActiveDataConnectionFeatureFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.DataConnection
{
    /// <summary>
    /// Creates an active FTP data connection.
    /// </summary>
    public class ActiveDataConnectionFeatureFactory
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        [NotNull]
        [ItemNotNull]
        private readonly List<IFtpDataConnectionValidator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDataConnectionFeatureFactory"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="sslStreamWrapperFactory">The SSL stream wrapper factory.</param>
        /// <param name="validators">An enumeration of FTP connection validators.</param>
        public ActiveDataConnectionFeatureFactory(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull, ItemNotNull] IEnumerable<IFtpDataConnectionValidator> validators)
        {
            _connectionAccessor = connectionAccessor;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _validators = validators.ToList();
        }

        /// <summary>
        /// Creates a <see cref="IFtpDataConnectionFeature"/> implementation for an active FTP data connection.
        /// </summary>
        /// <param name="ftpCommand">The FTP command that initiated the creation of the feature.</param>
        /// <param name="portAddress">The address the client wants the FTP server to connect to.</param>
        /// <param name="dataPort">The source port the server should use to connect to the client.</param>
        /// <returns>The task returning the new FTP data connection feature.</returns>
        [NotNull]
        [ItemNotNull]
        public Task<IFtpDataConnectionFeature> CreateFeatureAsync(
            [NotNull] FtpCommand ftpCommand,
            [NotNull] Address portAddress,
            int? dataPort)
        {
            var connection = _connectionAccessor.FtpConnection;
            var connectionFeature = connection.Features.Get<IConnectionFeature>();

#if NETSTANDARD1_3
            var client = new TcpClient(connectionFeature.LocalEndPoint.AddressFamily);
#else
            TcpClient client;
            if (dataPort != null)
            {
                var localEndPoint = new IPEndPoint(connectionFeature.LocalEndPoint.Address, dataPort.Value);
                client = new TcpClient(localEndPoint)
                {
                    ExclusiveAddressUse = false,
                };
            }
            else
            {
                client = new TcpClient(connectionFeature.LocalEndPoint.AddressFamily);
            }
#endif

            var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();
            return Task.FromResult<IFtpDataConnectionFeature>(
                new ActiveDataConnectionFeature(
                    client,
                    portAddress,
                    secureConnectionFeature,
                    _sslStreamWrapperFactory,
                    _validators,
                    ftpCommand,
                    connection));
        }

        private class ActiveDataConnectionFeature : IFtpDataConnectionFeature
        {
            [NotNull]
            private readonly Address _portAddress;

            [NotNull]
            private readonly ISecureConnectionFeature _secureConnectionFeature;

            [NotNull]
            private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

            [NotNull]
            private readonly TcpClient _client;

            [NotNull]
            [ItemNotNull]
            private readonly List<IFtpDataConnectionValidator> _validators;

            [NotNull]
            private readonly IFtpConnection _ftpConnection;

            public ActiveDataConnectionFeature(
                [NotNull] TcpClient client,
                [NotNull] Address portAddress,
                [NotNull] ISecureConnectionFeature secureConnectionFeature,
                [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
                [NotNull] [ItemNotNull] List<IFtpDataConnectionValidator> validators,
                [NotNull] FtpCommand command,
                [NotNull] IFtpConnection ftpConnection)
            {
                _portAddress = portAddress;
                _secureConnectionFeature = secureConnectionFeature;
                _sslStreamWrapperFactory = sslStreamWrapperFactory;
                _validators = validators;
                _ftpConnection = ftpConnection;
                Command = command;
                _client = client;
            }

            /// <inheritdoc />
            public FtpCommand Command { get; }

            /// <inheritdoc />
            public async Task<IFtpDataConnection> GetDataConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
            {
                var connectTask = _client.ConnectAsync(_portAddress.IPAddress, _portAddress.Port);
                var result = await Task.WhenAny(connectTask, Task.Delay(timeout, cancellationToken))
                   .ConfigureAwait(false);

                if (result != connectTask)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Opening data connection was aborted", cancellationToken);
                    }

                    throw new TimeoutException();
                }

                Stream stream = _client.GetStream();

                if (_secureConnectionFeature.CreateEncryptedStream != null)
                {
                    stream = await _secureConnectionFeature.CreateEncryptedStream(stream)
                       .ConfigureAwait(false);
                }

                var dataConnection = new ActiveDataConnection(_client, stream, _sslStreamWrapperFactory);
                foreach (var validator in _validators)
                {
                    var validationResult = await validator.IsValidAsync(_ftpConnection, this, dataConnection, cancellationToken)
                       .ConfigureAwait(false);
                    if (validationResult != ValidationResult.Success && validationResult != null)
                    {
                        throw new ValidationException(validationResult.ErrorMessage);
                    }
                }

                return dataConnection;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _client.Dispose();
            }

            private class ActiveDataConnection : IFtpDataConnection
            {
                [NotNull]
                private readonly TcpClient _client;

                [NotNull]
                private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

                private bool _closed;

                public ActiveDataConnection(
                    [NotNull] TcpClient client,
                    [NotNull] Stream stream,
                    [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory)
                {
                    _client = client;
                    _sslStreamWrapperFactory = sslStreamWrapperFactory;
                    Stream = stream;
                    LocalAddress = (IPEndPoint)client.Client.LocalEndPoint;
                    RemoteAddress = (IPEndPoint)client.Client.RemoteEndPoint;
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
                    _client.Dispose();
                }
            }
        }
    }
}
