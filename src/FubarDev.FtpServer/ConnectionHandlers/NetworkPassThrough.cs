// <copyright file="NetworkPassThrough.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public class NetworkPassThrough : ICommunicationService
    {
        [NotNull]
        private readonly PipeReader _reader;

        [NotNull]
        private readonly PipeWriter _writer;

        private readonly CancellationToken _connectionClosed;

        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        [NotNull]
        private CancellationTokenSource _jobPaused = new CancellationTokenSource();

        [NotNull]
        private Task _task = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkPassThrough"/> class.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="connectionClosed">Cancellation token for a closed connection.</param>
        public NetworkPassThrough(
            [NotNull] PipeReader reader,
            [NotNull] PipeWriter writer,
            CancellationToken connectionClosed)
        {
            _reader = reader;
            _writer = writer;
            _connectionClosed = connectionClosed;
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

            _task = PassThroughAsync(
                _reader,
                _writer,
                _connectionClosed,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running && Status != ConnectionStatus.Stopped && Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, {ConnectionStatus.Stopped}, or {ConnectionStatus.Paused}, but was {Status}.");
            }

            _jobStopped.Cancel();
            _reader.CancelPendingRead();

            return _task;
        }

        /// <inheritdoc />
        public Task PauseAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, but was {Status}.");
            }

            _jobPaused.Cancel();
            _reader.CancelPendingRead();

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

            _task = PassThroughAsync(
                _reader,
                _writer,
                _connectionClosed,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        private static async Task PassThroughAsync(
            PipeReader reader,
            PipeWriter writer,
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

                    var buffer = readResult.Buffer;
                    var position = buffer.Start;

                    while (buffer.TryGet(ref position, out var memory))
                    {
                        // Don't use the cancellation token source from above. Otherwise
                        // data might be lost.
                        await writer.WriteAsync(memory, CancellationToken.None)
                           .ConfigureAwait(false);
                    }

                    reader.AdvanceTo(buffer.End);

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
    }
}
