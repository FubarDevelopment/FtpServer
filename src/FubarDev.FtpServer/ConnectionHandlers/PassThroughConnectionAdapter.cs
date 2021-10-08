// <copyright file="PassThroughConnectionAdapter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Networking;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Connection adapter that passes data from one pipe to another.
    /// </summary>
    internal class PassThroughConnectionAdapter : IFtpConnectionAdapter
    {
        private readonly IPausableFtpService _transmitService;
        private readonly IPausableFtpService _receiverService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassThroughConnectionAdapter"/> class.
        /// </summary>
        /// <param name="socketPipe">The pipe for the socket.</param>
        /// <param name="connectionPipe">The pipe for the <see cref="IFtpConnection"/>.</param>
        /// <param name="connectionClosed">A cancellation token for a closed connection.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public PassThroughConnectionAdapter(
            IDuplexPipe socketPipe,
            IDuplexPipe connectionPipe,
            CancellationToken connectionClosed,
            ILoggerFactory? loggerFactory = null)
        {
            _receiverService = new NonClosingNetworkPassThrough(
                socketPipe.Input,
                connectionPipe.Output,
                connectionClosed,
                loggerFactory?.CreateLogger(typeof(PassThroughConnectionAdapter).FullName + ".Receiver"));
            _transmitService = new NonClosingNetworkPassThrough(
                connectionPipe.Input,
                socketPipe.Output,
                connectionClosed,
                loggerFactory?.CreateLogger(typeof(PassThroughConnectionAdapter).FullName + ".Transmitter"));
        }

        /// <inheritdoc />
        public IFtpService Sender => _transmitService;

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

        private class NonClosingNetworkPassThrough : PassThroughService
        {
            public NonClosingNetworkPassThrough(
                PipeReader reader,
                PipeWriter writer,
                CancellationToken connectionClosed,
                ILogger? logger = null)
                : base(reader, writer, connectionClosed, logger)
            {
            }

            /// <inheritdoc />
            protected override Task OnCloseAsync(Exception? exception, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
