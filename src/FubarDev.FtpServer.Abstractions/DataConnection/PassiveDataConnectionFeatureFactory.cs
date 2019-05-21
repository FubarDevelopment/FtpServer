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

using JetBrains.Annotations;

namespace FubarDev.FtpServer.DataConnection
{
    /// <summary>
    /// Creates a passive FTP data connection.
    /// </summary>
    public class PassiveDataConnectionFeatureFactory
    {
        [NotNull]
        private readonly IPasvListenerFactory _pasvListenerFactory;

        [NotNull]
        private readonly IFtpConnectionAccessor _connectionAccessor;

        [NotNull]
        [ItemNotNull]
        private readonly List<IFtpDataConnectionValidator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveDataConnectionFeatureFactory"/> class.
        /// </summary>
        /// <param name="pasvListenerFactory">The PASV listener factory.</param>
        /// <param name="connectionAccessor">The FTP connection accessor.</param>
        /// <param name="validators">An enumeration of FTP connection validators.</param>
        public PassiveDataConnectionFeatureFactory(
            [NotNull] IPasvListenerFactory pasvListenerFactory,
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] [ItemNotNull] IEnumerable<IFtpDataConnectionValidator> validators)
        {
            _pasvListenerFactory = pasvListenerFactory;
            _connectionAccessor = connectionAccessor;
            _validators = validators.ToList();
        }

        /// <summary>
        /// Creates a new <see cref="IFtpDataConnectionFeature"/> instance.
        /// </summary>
        /// <param name="ftpCommand">The FTP command that initiated the creation of the feature.</param>
        /// <param name="desiredPort">The desired port that the server should listen on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the <see cref="IFtpDataConnectionFeature"/> instance.</returns>
        [NotNull]
        [ItemNotNull]
        public async Task<IFtpDataConnectionFeature> CreateFeatureAsync(
            [NotNull] FtpCommand ftpCommand,
            int desiredPort,
            CancellationToken cancellationToken)
        {
            var connection = _connectionAccessor.FtpConnection;
            var listener = await _pasvListenerFactory.CreateTcpListenerAsync(
                connection,
                desiredPort,
                cancellationToken);
            return new PassiveDataConnectionFeature(listener, _validators, ftpCommand, connection);
        }

        private class PassiveDataConnectionFeature : IFtpDataConnectionFeature
        {
            [NotNull]
            private readonly IPasvListener _listener;

            [NotNull]
            [ItemNotNull]
            private readonly List<IFtpDataConnectionValidator> _validators;

            [NotNull]
            private readonly IFtpConnection _ftpConnection;

            public PassiveDataConnectionFeature(
                [NotNull] IPasvListener listener,
                [NotNull] [ItemNotNull] List<IFtpDataConnectionValidator> validators,
                [NotNull] FtpCommand command,
                [NotNull] IFtpConnection ftpConnection)
            {
                _listener = listener;
                _validators = validators;
                _ftpConnection = ftpConnection;
                Command = command;
            }

            /// <inheritdoc />
            public FtpCommand Command { get; }

            /// <inheritdoc />
            public async Task<IFtpDataConnection> GetDataConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
            {
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
                        var validationResult = await validator.IsValidAsync(_ftpConnection, this, dataConnection, cancellationToken)
                           .ConfigureAwait(false);
                        if (validationResult != ValidationResult.Success && validationResult != null)
                        {
                            throw new ValidationException(validationResult.ErrorMessage);
                        }
                    }

                    return dataConnection;
                }
                finally
                {
                    _listener.Dispose();
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _listener.Dispose();
            }

            private class PassiveDataConnection : IFtpDataConnection
            {
                [NotNull]
                private readonly TcpClient _client;

                private bool _closed;

                public PassiveDataConnection(
                    [NotNull] TcpClient client)
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
                public Task CloseAsync(CancellationToken cancellationToken)
                {
                    if (_closed)
                    {
                        return Task.CompletedTask;
                    }

                    _closed = true;
                    _client.Dispose();
                    return Task.CompletedTask;
                }
            }
        }
    }
}
