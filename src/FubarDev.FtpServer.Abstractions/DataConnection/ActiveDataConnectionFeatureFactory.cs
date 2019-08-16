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
        [ItemNotNull]
        private readonly List<IFtpDataConnectionValidator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDataConnectionFeatureFactory"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="validators">An enumeration of FTP connection validators.</param>
        public ActiveDataConnectionFeatureFactory(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull, ItemNotNull] IEnumerable<IFtpDataConnectionValidator> validators)
        {
            _connectionAccessor = connectionAccessor;
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
        [Obsolete("Use the overload with IPEndPoint as address instead.")]
        public Task<IFtpDataConnectionFeature> CreateFeatureAsync(
            [CanBeNull] FtpCommand ftpCommand,
            [NotNull] Address portAddress,
            int? dataPort)
        {
            var connection = _connectionAccessor.FtpConnection;
            var connectionFeature = connection.Features.Get<IConnectionFeature>();

            IPEndPoint localEndPoint;
            if (dataPort != null)
            {
                localEndPoint = new IPEndPoint(connectionFeature.LocalEndPoint.Address, dataPort.Value);
            }
            else
            {
                localEndPoint = new IPEndPoint(connectionFeature.LocalEndPoint.Address, 0);
            }

            var address = portAddress.IPAddress ?? connectionFeature.RemoteEndPoint.Address;
            var portEndPoint = new IPEndPoint(address, portAddress.Port);
            return Task.FromResult<IFtpDataConnectionFeature>(
                new ActiveDataConnectionFeature(
                    localEndPoint,
                    portEndPoint,
                    _validators,
                    ftpCommand,
                    connection));
        }

        /// <summary>
        /// Creates a <see cref="IFtpDataConnectionFeature"/> implementation for an active FTP data connection.
        /// </summary>
        /// <param name="ftpCommand">The FTP command that initiated the creation of the feature.</param>
        /// <param name="portEndPoint">The address the client wants the FTP server to connect to.</param>
        /// <param name="dataPort">The source port the server should use to connect to the client.</param>
        /// <returns>The task returning the new FTP data connection feature.</returns>
        [NotNull]
        [ItemNotNull]
        public Task<IFtpDataConnectionFeature> CreateFeatureAsync(
            [CanBeNull] FtpCommand ftpCommand,
            [NotNull] IPEndPoint portEndPoint,
            int? dataPort)
        {
            var connection = _connectionAccessor.FtpConnection;
            var connectionFeature = connection.Features.Get<IConnectionFeature>();

            var localEndPoint = dataPort != null
                ? new IPEndPoint(connectionFeature.LocalEndPoint.Address, dataPort.Value)
                : new IPEndPoint(connectionFeature.LocalEndPoint.Address, 0);

            return Task.FromResult<IFtpDataConnectionFeature>(
                new ActiveDataConnectionFeature(
                    localEndPoint,
                    portEndPoint,
                    _validators,
                    ftpCommand,
                    connection));
        }

        private class ActiveDataConnectionFeature : IFtpDataConnectionFeature
        {
            [NotNull]
            private readonly IPEndPoint _portAddress;

            [NotNull]
            [ItemNotNull]
            private readonly List<IFtpDataConnectionValidator> _validators;

            [NotNull]
            private readonly IFtpConnection _ftpConnection;

            public ActiveDataConnectionFeature(
                [NotNull] IPEndPoint localEndPoint,
                [NotNull] IPEndPoint portAddress,
                [NotNull] [ItemNotNull] List<IFtpDataConnectionValidator> validators,
                [CanBeNull] FtpCommand command,
                [NotNull] IFtpConnection ftpConnection)
            {
                _portAddress = portAddress;
                _validators = validators;
                _ftpConnection = ftpConnection;
                LocalEndPoint = localEndPoint;
                Command = command;
            }

            /// <inheritdoc />
            public FtpCommand Command { get; }

            /// <inheritdoc />
            public IPEndPoint LocalEndPoint { get; }

            /// <inheritdoc />
            public async Task<IFtpDataConnection> GetDataConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
            {
#if NETSTANDARD1_3
                var client = new TcpClient(LocalEndPoint.AddressFamily);
#else
                TcpClient client;
                if (LocalEndPoint.Port != 0)
                {
                    client = new TcpClient(LocalEndPoint.AddressFamily)
                    {
                        ExclusiveAddressUse = false,
                    };

                    client.Client.Bind(LocalEndPoint);
                }
                else
                {
                    client = new TcpClient(LocalEndPoint);
                }
#endif

                var exceptions = new List<Exception>();
                var tries = 0;
                var startTime = DateTime.UtcNow;
                do
                {
                    try
                    {
                        tries += 1;
                        var connectTask = client.ConnectAsync(_portAddress.Address, _portAddress.Port);
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

                        try
                        {
                            await connectTask.ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                            await Task.Delay(20, cancellationToken).ConfigureAwait(false);
                            await Task.Yield();
                            continue;
                        }

                        var dataConnection = new ActiveDataConnection(client);
                        foreach (var validator in _validators)
                        {
                            var validationResult = await validator.ValidateAsync(_ftpConnection, this, dataConnection, cancellationToken)
                               .ConfigureAwait(false);
                            if (validationResult != ValidationResult.Success && validationResult != null)
                            {
                                throw new ValidationException(validationResult.ErrorMessage);
                            }
                        }

                        return dataConnection;
                    }
                    catch (Exception) when (CloseTcpClient(client))
                    {
                        // Will never be executed!
                        throw;
                    }
                }
                while (tries < 5 || (DateTime.UtcNow - startTime) < timeout);

                throw new AggregateException(exceptions);
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            private bool CloseTcpClient(TcpClient client)
            {
                client.Dispose();
                return false;
            }

            private class ActiveDataConnection : IFtpDataConnection
            {
                [NotNull]
                private readonly TcpClient _client;

                private bool _closed;

                public ActiveDataConnection(
                    [NotNull] TcpClient client)
                {
                    _client = client;
                    Stream = client.GetStream();
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

                    await Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    _client.Dispose();
                }
            }
        }
    }
}
