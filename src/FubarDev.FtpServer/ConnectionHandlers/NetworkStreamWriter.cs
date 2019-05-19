// <copyright file="NetworkStreamWriter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Reads from a pipe and writes to a stream.
    /// </summary>
    public class NetworkStreamWriter : INetworkStreamService
    {
        [NotNull]
        private readonly PipeReader _pipeReader;

        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        private readonly CancellationToken _connectionClosed;

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
        public NetworkStreamWriter(
            [NotNull] Stream stream,
            [NotNull] PipeReader pipeReader,
            CancellationToken connectionClosed)
        {
            Stream = stream;
            _pipeReader = pipeReader;
            _connectionClosed = connectionClosed;
        }

        /// <inheritdoc />
        public Stream Stream { get; set; }

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
                Stream,
                _pipeReader,
                _connectionClosed,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running && Status != ConnectionStatus.Stopped)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, but was {Status}.");
            }

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

            _jobPaused.Cancel();

            await _task.ConfigureAwait(false);
            await FlushAsync(Stream, _pipeReader, cancellationToken);
        }

        /// <inheritdoc />
        public Task ContinueAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Paused}, but was {Status}.");
            }

            _jobPaused = new CancellationTokenSource();

            _task = SendPipelineAsync(
                Stream,
                _pipeReader,
                _connectionClosed,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        [NotNull]
        private static async Task FlushAsync(
            [NotNull] Stream stream,
            [NotNull] PipeReader reader,
            CancellationToken cancellationToken)
        {
            while (reader.TryRead(out var readResult))
            {
                await SendDataToStream(readResult.Buffer, stream, cancellationToken)
                   .ConfigureAwait(false);
            }
        }

        private static async Task SendPipelineAsync(
            [NotNull] Stream stream,
            [NotNull] PipeReader reader,
            CancellationToken connectionClosed,
            CancellationToken jobStopped,
            CancellationToken jobPaused,
            [NotNull] IProgress<ConnectionStatus> statusProgress)
        {
            var globalCts = CancellationTokenSource.CreateLinkedTokenSource(connectionClosed, jobStopped, jobPaused);

            statusProgress.Report(ConnectionStatus.Running);

            Exception exception = null;
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                try
                {
                    var readResult = await reader.ReadAsync(globalCts.Token)
                       .ConfigureAwait(false);

                    // Don't use the cancellation token source from above. Otherwise
                    // data might be lost.
                    await SendDataToStream(readResult.Buffer, stream, CancellationToken.None)
                       .ConfigureAwait(false);

                    reader.AdvanceTo(readResult.Buffer.End);

                    if (readResult.IsCanceled || readResult.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex.IsOperationCancelledException())
                {
                    // The job was aborted by one of the three cancellation tokens.
                    break;
                }
                catch (Exception ex)
                {
                    // Log?.LogError(ex, ex.Message);
                    exception = ex;
                    break;
                }
            }

            // Don't call Complete() when the job was just paused.
            if (jobPaused.IsCancellationRequested)
            {
                statusProgress.Report(ConnectionStatus.Paused);
                return;
            }

            // Tell the PipeReader that there's no more data coming
            reader.Complete(exception);

            statusProgress.Report(ConnectionStatus.Stopped);
        }

        [NotNull]
        private static async Task SendDataToStream(
            ReadOnlySequence<byte> buffer,
            [NotNull] Stream stream,
            CancellationToken cancellationToken)
        {
            var position = buffer.Start;

            while (buffer.TryGet(ref position, out var memory))
            {
                var streamBuffer = memory.ToArray();
                await stream.WriteAsync(streamBuffer, 0, streamBuffer.Length, cancellationToken)
                   .ConfigureAwait(false);
            }

            await stream.FlushAsync(cancellationToken);
        }
    }
}
