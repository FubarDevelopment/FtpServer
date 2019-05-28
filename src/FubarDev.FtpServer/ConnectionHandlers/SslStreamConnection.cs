// <copyright file="SslStreamConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// A communication service that injects an SSL stream between the socket and the connection pipe.
    /// </summary>
    internal class SslStreamConnection : ICommunicationService
    {
        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        private readonly IDuplexPipe _socketPipe;

        [NotNull]
        private readonly IDuplexPipe _connectionPipe;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        [NotNull]
        private readonly X509Certificate2 _certificate;

        private readonly CancellationToken _connectionClosed;

        [CanBeNull]
        private readonly ILoggerFactory _loggerFactory;

        [CanBeNull]
        private SslCommunicationInfo _info;

        public SslStreamConnection(
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull] X509Certificate2 certificate,
            CancellationToken connectionClosed,
            [CanBeNull] ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _socketPipe = socketPipe;
            _connectionPipe = connectionPipe;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _certificate = certificate;
            _connectionClosed = connectionClosed;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public IPausableCommunicationService Sender
            => _info?.TransmitterService
                ?? throw new InvalidOperationException("Sender can only be accessed when the connection service was started.");

        /// <inheritdoc />
        public IPausableCommunicationService Receiver
            => _info?.ReceiverService
                ?? throw new InvalidOperationException("Receiver can only be accessed when the connection service was started.");

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var rawStream = new RawStream(
                _socketPipe.Input,
                _socketPipe.Output,
                _serviceProvider.GetService<ILogger<RawStream>>());
            var sslStream = await _sslStreamWrapperFactory.WrapStreamAsync(rawStream, false, _certificate, cancellationToken)
               .ConfigureAwait(false);
            var transmitterService = new NonClosingNetworkStreamWriter(
                sslStream,
                _connectionPipe.Input,
                _connectionClosed,
                _loggerFactory?.CreateLogger(typeof(SslStreamConnection).FullName + ":Transmitter"));
            var receiverService = new NonClosingNetworkStreamReader(
                sslStream,
                _connectionPipe.Output,
                _connectionClosed,
                _loggerFactory?.CreateLogger(typeof(SslStreamConnection).FullName + ":Receiver"));
            var info = new SslCommunicationInfo(transmitterService, receiverService, sslStream);
            _info = info;

            await info.TransmitterService.StartAsync(cancellationToken)
               .ConfigureAwait(false);
            await info.ReceiverService.StartAsync(cancellationToken)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_info == null)
            {
                // Service wasn't started yet!
                return;
            }

            var info = _info;

            var receiverStopTask = info.ReceiverService.StopAsync(cancellationToken);
            var transmitterStopTask = info.TransmitterService.StopAsync(cancellationToken);

            await Task.WhenAll(receiverStopTask, transmitterStopTask)
               .ConfigureAwait(false);

            await _sslStreamWrapperFactory.CloseStreamAsync(info.SslStream, cancellationToken)
               .ConfigureAwait(false);

            _info = null;
        }

        private class SslCommunicationInfo
        {
            public SslCommunicationInfo(
                [NotNull] IPausableCommunicationService transmitterService,
                [NotNull] IPausableCommunicationService receiverService,
                [NotNull] Stream sslStream)
            {
                TransmitterService = transmitterService;
                ReceiverService = receiverService;
                SslStream = sslStream;
            }

            [NotNull]
            public IPausableCommunicationService TransmitterService { get; }

            [NotNull]
            public IPausableCommunicationService ReceiverService { get; }

            [NotNull]
            public Stream SslStream { get; }
        }

        private class NonClosingNetworkStreamReader : NetworkStreamReader
        {
            public NonClosingNetworkStreamReader(
                [NotNull] Stream stream,
                [NotNull] PipeWriter pipeWriter,
                CancellationToken connectionClosed,
                [CanBeNull] ILogger logger = null)
                : base(stream, pipeWriter, connectionClosed, logger)
            {
            }

            /// <inheritdoc />
            protected override Task OnCloseAsync(Exception exception, CancellationToken cancellationToken)
            {
                // Do nothing
                return Task.CompletedTask;
            }
        }

        private class NonClosingNetworkStreamWriter : NetworkStreamWriter
        {
            public NonClosingNetworkStreamWriter(
                [NotNull] Stream stream,
                [NotNull] PipeReader pipeReader,
                CancellationToken connectionClosed,
                [CanBeNull] ILogger logger = null)
                : base(stream, pipeReader, connectionClosed, logger)
            {
            }

            /// <inheritdoc />
            protected override Task OnCloseAsync(Exception exception, CancellationToken cancellationToken)
            {
                // Do nothing
                return Task.CompletedTask;
            }
        }
    }
}
