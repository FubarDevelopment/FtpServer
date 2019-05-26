// <copyright file="SslStreamConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Buffers;
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
    public class SslStreamConnection : IBasicCommunicationService, ISenderService, IReceiverService
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
        private SslCommunicationInfo _info;

        public SslStreamConnection(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull] X509Certificate2 certificate,
            CancellationToken connectionClosed)
        {
            _serviceProvider = serviceProvider;
            _socketPipe = socketPipe;
            _connectionPipe = connectionPipe;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _certificate = certificate;
            _connectionClosed = connectionClosed;
        }

        /// <inheritdoc />
        public ICommunicationService Sender
            => _info?.TransmitterService
                ?? throw new InvalidOperationException("Sender can only be accessed when the connection service was started.");

        /// <inheritdoc />
        public ICommunicationService Receiver
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
            var transmitterService = new NetworkStreamWriter(sslStream, _connectionPipe.Input, _connectionClosed);
            var receiverService = new NetworkStreamReader(sslStream, _connectionPipe.Output, _connectionClosed);
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

            _socketPipe.Input.CancelPendingRead();
            _connectionPipe.Input.CancelPendingRead();

            await Task.WhenAll(receiverStopTask, transmitterStopTask)
               .ConfigureAwait(false);

            await _sslStreamWrapperFactory.CloseStreamAsync(info.SslStream, cancellationToken)
               .ConfigureAwait(false);

            _info = null;
        }

        private class SslCommunicationInfo
        {
            public SslCommunicationInfo(
                [NotNull] ICommunicationService transmitterService,
                [NotNull] ICommunicationService receiverService,
                [NotNull] Stream sslStream)
            {
                TransmitterService = transmitterService;
                ReceiverService = receiverService;
                SslStream = sslStream;
            }

            [NotNull]
            public ICommunicationService TransmitterService { get; }

            [NotNull]
            public ICommunicationService ReceiverService { get; }

            [NotNull]
            public Stream SslStream { get; }
        }

        /// <summary>
        /// Reads from a stream and writes into a pipeline.
        /// </summary>
        private class NetworkStreamReader : ICommunicationService
        {
            [NotNull]
            private readonly Stream _stream;

            [NotNull]
            private readonly PipeWriter _pipeWriter;

            [NotNull]
            private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

            private readonly CancellationToken _connectionClosed;

            [CanBeNull]
            private readonly ILogger<NetworkStreamReader> _logger;

            [NotNull]
            private CancellationTokenSource _jobPaused = new CancellationTokenSource();

            [NotNull]
            private Task _task = Task.CompletedTask;

            /// <summary>
            /// Initializes a new instance of the <see cref="NetworkStreamReader"/> class.
            /// </summary>
            /// <param name="stream">The stream to read from.</param>
            /// <param name="pipeWriter">The pipeline to write to.</param>
            /// <param name="connectionClosed">Cancellation token source for a closed connection.</param>
            /// <param name="logger">The logger.</param>
            public NetworkStreamReader(
                [NotNull] Stream stream,
                [NotNull] PipeWriter pipeWriter,
                CancellationToken connectionClosed,
                [CanBeNull] ILogger<NetworkStreamReader> logger = null)
            {
                _stream = stream;
                _pipeWriter = pipeWriter;
                _connectionClosed = connectionClosed;
                _logger = logger;
            }

            /// <inheritdoc />
            public ConnectionStatus Status { get; private set; } = ConnectionStatus.ReadyToRun;

            /// <inheritdoc />
            public Task StartAsync(CancellationToken cancellationToken)
            {
                if (Status != ConnectionStatus.ReadyToRun)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
                }

                _task = FillPipelineAsync(
                    _stream,
                    _pipeWriter,
                    _connectionClosed,
                    _jobStopped.Token,
                    _jobPaused.Token,
                    new Progress<ConnectionStatus>(status => Status = status),
                    _logger);

                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public Task StopAsync(CancellationToken cancellationToken)
            {
                if (Status != ConnectionStatus.Running && Status != ConnectionStatus.Stopped && Status != ConnectionStatus.Paused)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, {ConnectionStatus.Stopped}, or {ConnectionStatus.Paused}, but was {Status}.");
                }

                _logger?.LogTrace("Signalling STOP");
                _jobStopped.Cancel();

                return _task;
            }

            /// <inheritdoc />
            public Task PauseAsync(CancellationToken cancellationToken)
            {
                if (Status != ConnectionStatus.Running)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, but was {Status}.");
                }

                _logger?.LogTrace("Signalling PAUSE");
                _jobPaused.Cancel();

                return _task;
            }

            /// <inheritdoc />
            public Task ContinueAsync(CancellationToken cancellationToken)
            {
                if (Status == ConnectionStatus.Stopped)
                {
                    // Stay stopped!
                    return Task.CompletedTask;
                }

                if (Status != ConnectionStatus.Paused)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.Paused}, but was {Status}.");
                }

                _jobPaused = new CancellationTokenSource();

                _task = FillPipelineAsync(
                    _stream,
                    _pipeWriter,
                    _connectionClosed,
                    _jobStopped.Token,
                    _jobPaused.Token,
                    new Progress<ConnectionStatus>(status => Status = status),
                    _logger);

                return Task.CompletedTask;
            }

            [NotNull]
            private static async Task FillPipelineAsync(
                [NotNull] Stream stream,
                [NotNull] PipeWriter writer,
                CancellationToken connectionClosed,
                CancellationToken jobStopped,
                CancellationToken jobPaused,
                IProgress<ConnectionStatus> statusProgress,
                [CanBeNull] ILogger logger)
            {
                var globalCts = CancellationTokenSource.CreateLinkedTokenSource(connectionClosed, jobStopped, jobPaused);

                statusProgress.Report(ConnectionStatus.Running);

                logger?.LogTrace("Starting filling pipeline");

                var buffer = new byte[1024];
                while (true)
                {
                    // Allocate at least 512 bytes from the PipeWriter
                    var memory = writer.GetMemory(buffer.Length);
                    try
                    {
                        logger?.LogTrace("Start reading with buffer size = {bufferSize}", buffer.Length);

                        var readTask = stream
                           .ReadAsync(buffer, 0, buffer.Length, globalCts.Token);

                        var resultTask = await Task.WhenAny(readTask, Task.Delay(-1, globalCts.Token))
                           .ConfigureAwait(false);
                        if (resultTask != readTask)
                        {
                            logger?.LogTrace("Cancelled through Task.Delay");
                            break;
                        }

                        var bytesRead = readTask.Result;

                        logger?.LogTrace("Bytes read = {bytesRead}", bytesRead);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        buffer.AsSpan(0, bytesRead).CopyTo(memory.Span);

                        // Tell the PipeWriter how much was read from the Socket
                        writer.Advance(bytesRead);

                        // Make the data available to the PipeReader.
                        // Don't use the cancellation token source from above. Otherwise
                        // data might be lost.
                        var result = await writer.FlushAsync(CancellationToken.None);
                        if (result.IsCompleted)
                        {
                            logger?.LogTrace("Completed");
                            break;
                        }

                        if (result.IsCanceled)
                        {
                            logger?.LogTrace("Cancelled");
                            break;
                        }
                    }
                    catch (Exception ex) when (ex.IsIOException())
                    {
                        // Connection closed?
                        logger?.LogTrace("Connection closed by client");
                        break;
                    }
                    catch (Exception ex) when (ex.IsOperationCancelledException())
                    {
                        // The job was aborted by one of the three cancellation tokens.
                        logger?.LogTrace("Operation cancelled");
                        break;
                    }
                }

                // Don't call Complete() when the job was just paused.
                if (jobPaused.IsCancellationRequested)
                {
                    logger?.LogTrace("Paused reader");
                    statusProgress.Report(ConnectionStatus.Paused);
                    return;
                }

                // Change the status
                statusProgress.Report(ConnectionStatus.Stopped);

                logger?.LogTrace("Stopped reader");
            }
        }

        /// <summary>
        /// Reads from a pipe and writes to a stream.
        /// </summary>
        private class NetworkStreamWriter : ICommunicationService
        {
            [NotNull]
            private readonly Stream _stream;

            [NotNull]
            private readonly PipeReader _pipeReader;

            [NotNull]
            private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

            private readonly CancellationToken _connectionClosed;

            [CanBeNull]
            private readonly ILogger<NetworkStreamWriter> _logger;

            [NotNull]
            private CancellationTokenSource _jobPaused = new CancellationTokenSource();

            [NotNull]
            private Task _task = Task.CompletedTask;

            /// <summary>
            /// Initializes a new instance of the <see cref="NetworkStreamWriter"/> class.
            /// </summary>
            /// <param name="stream">The stream to write to.</param>
            /// <param name="pipeReader">The pipe to read from.</param>
            /// <param name="connectionClosed">Cancellation token for a closed connection.</param>
            /// <param name="logger">The logger.</param>
            public NetworkStreamWriter(
                [NotNull] Stream stream,
                [NotNull] PipeReader pipeReader,
                CancellationToken connectionClosed,
                [CanBeNull] ILogger<NetworkStreamWriter> logger = null)
            {
                _stream = stream;
                _pipeReader = pipeReader;
                _connectionClosed = connectionClosed;
                _logger = logger;
            }

            /// <inheritdoc />
            public ConnectionStatus Status { get; private set; } = ConnectionStatus.ReadyToRun;

            /// <inheritdoc />
            public Task StartAsync(CancellationToken cancellationToken)
            {
                if (Status != ConnectionStatus.ReadyToRun)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
                }

                _task = SendPipelineAsync(
                    _stream,
                    _pipeReader,
                    _connectionClosed,
                    _jobStopped.Token,
                    _jobPaused.Token,
                    new Progress<ConnectionStatus>(status => Status = status),
                    _logger);

                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public Task StopAsync(CancellationToken cancellationToken)
            {
                if (Status != ConnectionStatus.Running && Status != ConnectionStatus.Stopped && Status != ConnectionStatus.Paused)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, {ConnectionStatus.Stopped}, or {ConnectionStatus.Paused}, but was {Status}.");
                }

                _logger?.LogTrace("Signalling STOP");
                _jobStopped.Cancel();

                return _task;
            }

            /// <inheritdoc />
            public async Task PauseAsync(CancellationToken cancellationToken)
            {
                if (Status != ConnectionStatus.Running)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, but was {Status}.");
                }

                _logger?.LogTrace("Signalling PAUSE");
                _jobPaused.Cancel();

                await _task.ConfigureAwait(false);

                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await FlushAsync(_stream, _pipeReader, cancellationToken, _logger).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (ex.IsIOException())
                {
                    // Ignored. Connection closed by client?
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(0, ex, "Flush failed with: {message}", ex.Message);
                }
            }

            /// <inheritdoc />
            public Task ContinueAsync(CancellationToken cancellationToken)
            {
                if (Status == ConnectionStatus.Stopped)
                {
                    // Stay stopped!
                    return Task.CompletedTask;
                }

                if (Status != ConnectionStatus.Paused)
                {
                    throw new InvalidOperationException($"Status must be {ConnectionStatus.Paused}, but was {Status}.");
                }

                _jobPaused = new CancellationTokenSource();

                _task = SendPipelineAsync(
                    _stream,
                    _pipeReader,
                    _connectionClosed,
                    _jobStopped.Token,
                    _jobPaused.Token,
                    new Progress<ConnectionStatus>(status => Status = status),
                    _logger);

                return Task.CompletedTask;
            }

            [NotNull]
            private static async Task FlushAsync(
                [NotNull] Stream stream,
                [NotNull] PipeReader reader,
                CancellationToken cancellationToken,
                [CanBeNull] ILogger logger)
            {
                logger?.LogTrace("Flushing");
                while (reader.TryRead(out var readResult))
                {
                    await SendDataToStream(readResult.Buffer, stream, cancellationToken, logger)
                       .ConfigureAwait(false);
                    reader.AdvanceTo(readResult.Buffer.End);
                }

                logger?.LogTrace("Flushed");
            }

            private static async Task SendPipelineAsync(
                [NotNull] Stream stream,
                [NotNull] PipeReader reader,
                CancellationToken connectionClosed,
                CancellationToken jobStopped,
                CancellationToken jobPaused,
                [NotNull] IProgress<ConnectionStatus> statusProgress,
                [CanBeNull] ILogger logger)
            {
                var globalCts = CancellationTokenSource.CreateLinkedTokenSource(connectionClosed, jobStopped, jobPaused);

                statusProgress.Report(ConnectionStatus.Running);

                logger?.LogTrace("Starting reading pipeline");

                while (true)
                {
                    // Allocate at least 512 bytes from the PipeWriter
                    try
                    {
                        logger?.LogTrace("Start reading response");
                        var readResult = await reader.ReadAsync(globalCts.Token)
                           .ConfigureAwait(false);

                        // Don't use the cancellation token source from above. Otherwise
                        // data might be lost.
                        await SendDataToStream(readResult.Buffer, stream, CancellationToken.None, logger)
                           .ConfigureAwait(false);

                        reader.AdvanceTo(readResult.Buffer.End);

                        if (readResult.IsCanceled || readResult.IsCompleted)
                        {
                            logger?.LogTrace("Was cancelled or completed.");
                            break;
                        }
                    }
                    catch (Exception ex) when (ex.IsOperationCancelledException())
                    {
                        // The job was aborted by one of the three cancellation tokens.
                        logger?.LogTrace("Operation cancelled");
                        break;
                    }
                }

                // Don't call Complete() when the job was just paused.
                if (jobPaused.IsCancellationRequested)
                {
                    logger?.LogTrace("Paused writer");
                    statusProgress.Report(ConnectionStatus.Paused);
                    return;
                }

                statusProgress.Report(ConnectionStatus.Stopped);

                logger?.LogTrace("Stopped writer");
            }

            [NotNull]
            private static async Task SendDataToStream(
                ReadOnlySequence<byte> buffer,
                [NotNull] Stream stream,
                CancellationToken cancellationToken,
                [CanBeNull] ILogger logger)
            {
                logger?.LogTrace("Start sending");
                var position = buffer.Start;

                while (buffer.TryGet(ref position, out var memory))
                {
                    var streamBuffer = memory.ToArray();
                    await stream.WriteAsync(streamBuffer, 0, streamBuffer.Length, cancellationToken)
                       .ConfigureAwait(false);
                }

                logger?.LogTrace("Flushing stream");
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
