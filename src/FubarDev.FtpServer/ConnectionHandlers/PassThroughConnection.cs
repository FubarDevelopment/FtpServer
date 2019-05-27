// <copyright file="PassThroughConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public class PassThroughConnection : ICommunicationService
    {
        [NotNull]
        private readonly IPausableCommunicationService _transmitService;

        [NotNull]
        private readonly IPausableCommunicationService _receiverService;

        public PassThroughConnection(
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            CancellationToken connectionClosed)
        {
            _receiverService = new NonClosingNetworkPassThrough(socketPipe.Input, connectionPipe.Output, connectionClosed);
            _transmitService = new NonClosingNetworkPassThrough(connectionPipe.Input, socketPipe.Output, connectionClosed);
        }

        /// <inheritdoc />
        public IPausableCommunicationService Sender => _transmitService;

        /// <inheritdoc />
        public IPausableCommunicationService Receiver => _receiverService;

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                _transmitService.StartAsync(cancellationToken),
                _receiverService.StartAsync(cancellationToken));
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                _transmitService.StopAsync(cancellationToken),
                _receiverService.StopAsync(cancellationToken));
        }

        private class NonClosingNetworkPassThrough : NetworkPassThrough
        {
            public NonClosingNetworkPassThrough(
                [NotNull] PipeReader reader,
                [NotNull] PipeWriter writer,
                CancellationToken connectionClosed,
                [CanBeNull] ILogger logger = null)
                : base(reader, writer, connectionClosed, logger)
            {
            }

            /// <inheritdoc />
            protected override Task OnCloseAsync(Exception exception, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
