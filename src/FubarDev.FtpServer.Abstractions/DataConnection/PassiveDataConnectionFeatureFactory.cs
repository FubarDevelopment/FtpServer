// <copyright file="PassiveDataConnectionFeatureFactory.cs" company="Fubar Development Junker">
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

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.DataConnection
{
    /// <summary>
    /// Creates a passive FTP data connection.
    /// </summary>
    public class PassiveDataConnectionFeatureFactory
    {
        private readonly IPasvListenerFactory _pasvListenerFactory;
        private readonly IFtpConnectionAccessor _connectionAccessor;
        private readonly ILogger<PassiveDataConnectionFeatureFactory>? _logger;
        private readonly List<IFtpDataConnectionValidator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveDataConnectionFeatureFactory"/> class.
        /// </summary>
        /// <param name="pasvListenerFactory">The PASV listener factory.</param>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="validators">An enumeration of FTP connection validators.</param>
        /// <param name="logger">The logger.</param>
        public PassiveDataConnectionFeatureFactory(
            IPasvListenerFactory pasvListenerFactory,
            IFtpConnectionAccessor connectionAccessor,
            IEnumerable<IFtpDataConnectionValidator> validators,
            ILogger<PassiveDataConnectionFeatureFactory>? logger = null)
        {
            _pasvListenerFactory = pasvListenerFactory;
            _connectionAccessor = connectionAccessor;
            _logger = logger;
            _validators = validators.ToList();
        }

        /// <summary>
        /// Creates a new <see cref="IFtpDataConnectionFeature"/> instance.
        /// </summary>
        /// <param name="ftpCommand">The FTP command that initiated the creation of the feature.</param>
        /// <param name="addressFamily">The address family for the address to be selected.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the <see cref="IFtpDataConnectionFeature"/> instance.</returns>
        public async Task<IFtpDataConnectionFeature> CreateFeatureAsync(
            FtpCommand ftpCommand,
            AddressFamily? addressFamily,
            CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;
            var listener = await _pasvListenerFactory.CreateTcpListenerAsync(
                connection,
                addressFamily,
                0,
                cancellationToken).ConfigureAwait(false);
            return new PassiveDataConnectionFeature(
                listener,
                _validators,
                ftpCommand,
                connection,
                listener.PasvEndPoint,
                _logger);
        }

        private class PassiveDataConnectionFeature : IFtpDataConnectionFeature
        {
            private readonly IPasvListener _listener;
            private readonly List<IFtpDataConnectionValidator> _validators;
            private readonly IFtpConnection _ftpConnection;
            private readonly ILogger? _logger;
            private IFtpDataConnection? _activeDataConnection;

            public PassiveDataConnectionFeature(
                IPasvListener listener,
                List<IFtpDataConnectionValidator> validators,
                FtpCommand? command,
                IFtpConnection ftpConnection,
                IPEndPoint localEndPoint,
                ILogger? logger)
            {
                _listener = listener;
                _validators = validators;
                _ftpConnection = ftpConnection;
                _logger = logger;
                LocalEndPoint = localEndPoint;
                Command = command;
            }

            /// <inheritdoc />
            public FtpCommand? Command { get; }

            /// <inheritdoc />
            public IPEndPoint LocalEndPoint { get; }

            /// <inheritdoc />
            public async Task<IFtpDataConnection> GetDataConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
            {
                if (_activeDataConnection != null && !_activeDataConnection.Closed)
                {
                    return _activeDataConnection;
                }

                try
                {
                    var connectTask = _listener.AcceptPasvClientAsync();
                    var result = await Task.WhenAny(connectTask, Task.Delay(timeout, cancellationToken))
                       .ConfigureAwait(false);

                    if (result != connectTask)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(
                                "Opening data connection was aborted",
                                cancellationToken);
                        }

                        throw new TimeoutException();
                    }

                    var client = await connectTask.ConfigureAwait(false);

                    var dataConnection = new PassiveDataConnection(client);
                    foreach (var validator in _validators)
                    {
                        var validationResult = await validator.ValidateAsync(_ftpConnection, this, dataConnection, cancellationToken)
                           .ConfigureAwait(false);
                        if (validationResult != ValidationResult.Success && validationResult != null)
                        {
                            throw new ValidationException(validationResult.ErrorMessage);
                        }
                    }

                    _logger?.LogDebug("Data connection accepted from {remoteIp}", dataConnection.RemoteAddress);

                    _activeDataConnection = dataConnection;
                    return dataConnection;
                }
                finally
                {
                    _listener.Dispose();
                }
            }

            /// <inheritdoc />
            public async Task DisposeAsync()
            {
                if (_activeDataConnection != null)
                {
                    await _activeDataConnection.CloseAsync(CancellationToken.None);
                }

                _listener.Dispose();
            }

            private class PassiveDataConnection : IFtpDataConnection
            {
                private readonly TcpClient _client;

                private bool _closed;

                public PassiveDataConnection(
                    TcpClient client)
                {
                    _client = client;
                    Stream = _client.GetStream();
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
                public bool Closed => _closed;

                /// <inheritdoc />
                public async Task CloseAsync(CancellationToken cancellationToken)
                {
                    if (_closed)
                    {
                        return;
                    }

                    _closed = true;

                    await Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
#if !NETSTANDARD1_3
                    _client.Close();
#endif
                    _client.Dispose();
                }
            }
        }
    }
}
