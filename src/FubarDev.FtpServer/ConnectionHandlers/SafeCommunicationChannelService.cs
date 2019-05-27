// <copyright file="SafeCommunicationChannelService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public class SafeCommunicationChannelService : ISafeCommunicationService
    {
        [NotNull]
        private readonly IDuplexPipe _socketPipe;

        [NotNull]
        private readonly IDuplexPipe _connectionPipe;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        private readonly CancellationToken _connectionClosed;

        [NotNull]
        private ICommunicationService _activeCommunicationService;

        public SafeCommunicationChannelService(
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull] IServiceProvider serviceProvider,
            CancellationToken connectionClosed)
        {
            _socketPipe = socketPipe;
            _connectionPipe = connectionPipe;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _serviceProvider = serviceProvider;
            _connectionClosed = connectionClosed;
            _activeCommunicationService = new PassThroughConnection(socketPipe, connectionPipe, connectionClosed);
        }

        /// <inheritdoc />
        public IPausableCommunicationService Sender => _activeCommunicationService.Sender;

        /// <inheritdoc />
        public IPausableCommunicationService Receiver => _activeCommunicationService.Receiver;

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken)
        {
            _activeCommunicationService = new PassThroughConnection(_socketPipe, _connectionPipe, _connectionClosed);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task EnableSslStreamAsync(X509Certificate2 certificate, CancellationToken cancellationToken)
        {
            _activeCommunicationService = new SslStreamConnection(
                _socketPipe,
                _connectionPipe,
                _serviceProvider,
                _sslStreamWrapperFactory,
                certificate,
                _connectionClosed);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _activeCommunicationService.StartAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _activeCommunicationService.StopAsync(cancellationToken);
        }
    }
}
