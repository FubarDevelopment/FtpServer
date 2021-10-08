// <copyright file="SslStreamConnectionAdapter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Networking;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// A connection adapter that injects an SSL stream between the socket and the connection pipe.
    /// </summary>
    internal class SslStreamConnectionAdapter : IFtpConnectionAdapter
    {
        private readonly IDuplexPipe _socketPipe;
        private readonly IDuplexPipe _connectionPipe;
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;
        private readonly X509Certificate _certificate;

        private readonly CancellationToken _connectionClosed;
        private readonly ILoggerFactory? _loggerFactory;

        private SslCommunicationInfo? _info;

        public SslStreamConnectionAdapter(
            IDuplexPipe socketPipe,
            IDuplexPipe connectionPipe,
            ISslStreamWrapperFactory sslStreamWrapperFactory,
            X509Certificate certificate,
            CancellationToken connectionClosed,
            ILoggerFactory? loggerFactory = null)
        {
            _socketPipe = socketPipe;
            _connectionPipe = connectionPipe;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _certificate = certificate;
            _connectionClosed = connectionClosed;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public IFtpService Sender
            => _info?.TransmitterService
                ?? throw new InvalidOperationException("Sender can only be accessed when the connection service was started.");

        /// <inheritdoc />
        public IPausableFtpService Receiver
            => _info?.ReceiverService
                ?? throw new InvalidOperationException("Receiver can only be accessed when the connection service was started.");

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var rawStream = new SimplePipeStream(
                _socketPipe.Input,
                _socketPipe.Output);
            var sslStream = await _sslStreamWrapperFactory.WrapStreamAsync(rawStream, false, _certificate, cancellationToken)
               .ConfigureAwait(false);

            var receiverLogger = _loggerFactory
              ?.CreateLogger(typeof(SslStreamConnectionAdapter).FullName + ".Receiver");
            var receiverService = new NonClosingNetworkStreamReader(
                sslStream,
                _connectionPipe.Output,
                _socketPipe.Input,
                _connectionClosed,
                receiverLogger);

            var transmitterLogger = _loggerFactory
              ?.CreateLogger(typeof(SslStreamConnectionAdapter).FullName + ".Transmitter");
            var transmitterService = new NonClosingNetworkStreamWriter(
                sslStream,
                _connectionPipe.Input,
                _connectionClosed,
                transmitterLogger);

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
                IPausableFtpService transmitterService,
                IPausableFtpService receiverService,
                Stream sslStream)
            {
                TransmitterService = transmitterService;
                ReceiverService = receiverService;
                SslStream = sslStream;
            }
            public IPausableFtpService TransmitterService { get; }
            public IPausableFtpService ReceiverService { get; }
            public Stream SslStream { get; }
        }

        private class NonClosingNetworkStreamReader : StreamPipeReaderService
        {
            private readonly PipeReader _socketPipeReader;

            public NonClosingNetworkStreamReader(
                Stream stream,
                PipeWriter pipeWriter,
                PipeReader socketPipeReader,
                CancellationToken connectionClosed,
                ILogger? logger = null)
                : base(stream, pipeWriter, connectionClosed, logger)
            {
                _socketPipeReader = socketPipeReader;
            }

            /// <inheritdoc />
            protected override Task OnPauseRequestedAsync(CancellationToken cancellationToken)
            {
                _socketPipeReader.CancelPendingRead();
                return base.OnPauseRequestedAsync(cancellationToken);
            }

            /// <inheritdoc />
            protected override Task OnCloseAsync(Exception? exception, CancellationToken cancellationToken)
            {
                // Do nothing
                return Task.CompletedTask;
            }
        }

        private class NonClosingNetworkStreamWriter : StreamPipeWriterService
        {
            public NonClosingNetworkStreamWriter(
                Stream stream,
                PipeReader pipeReader,
                CancellationToken connectionClosed,
                ILogger? logger = null)
                : base(stream, pipeReader, connectionClosed, logger)
            {
            }

            /// <inheritdoc />
            protected override Task OnCloseAsync(Exception? exception, CancellationToken cancellationToken)
            {
                // Do nothing
                return Task.CompletedTask;
            }

#if USE_SYNC_SSL_STREAM
            /// <inheritdoc />
            protected override Task WriteToStreamAsync(
                byte[] buffer,
                int offset,
                int length,
                CancellationToken cancellationToken)
            {
                // We have to use Write instead of WriteAsync, because
                // otherwise we might run into a deadlock.
                //
                // It **might** be related to the following issues:
                // https://github.com/dotnet/corefx/issues/5077
                // https://github.com/dotnet/corefx/issues/14698
                Stream.Write(buffer, offset, length);
                return Task.CompletedTask;
            }
#endif
        }
    }
}
