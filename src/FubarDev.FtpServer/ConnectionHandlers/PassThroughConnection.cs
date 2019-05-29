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
    /// <summary>
    /// Communication service that passes data from one pipe to another.
    /// </summary>
    internal class PassThroughConnection : ICommunicationService
    {
        [NotNull]
        private readonly IPausableFtpService _transmitService;

        [NotNull]
        private readonly IPausableFtpService _receiverService;

        public PassThroughConnection(
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            CancellationToken connectionClosed,
            [CanBeNull] ILoggerFactory loggerFactory)
        {
            _receiverService = new NonClosingNetworkPassThrough(
                socketPipe.Input,
                connectionPipe.Output,
                connectionClosed,
                loggerFactory?.CreateLogger(typeof(PassThroughConnection).FullName + ":Receiver"));
            _transmitService = new NonClosingNetworkPassThrough(
                connectionPipe.Input,
                socketPipe.Output,
                connectionClosed,
                loggerFactory?.CreateLogger(typeof(PassThroughConnection).FullName + ":Transmitter"));
        }

        /// <inheritdoc />
        public IPausableFtpService Sender => _transmitService;

        /// <inheritdoc />
        public IPausableFtpService Receiver => _receiverService;

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
