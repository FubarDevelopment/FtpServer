// <copyright file="NetworkStreamReader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Reads from a stream and writes into a pipeline.
    /// </summary>
    public class NetworkStreamReader : INetworkStreamService
    {
        [NotNull]
        private readonly PipeWriter _pipeWriter;

        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        private readonly CancellationTokenSource _connectionClosedCts;

        [NotNull]
        private CancellationTokenSource _jobPaused = new CancellationTokenSource();

        [NotNull]
        private Task _task = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkStreamReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="pipeWriter">The pipeline to write to.</param>
        /// <param name="connectionClosedCts">Cancellation token source for a closed connection.</param>
        public NetworkStreamReader(
            [NotNull] Stream stream,
            [NotNull] PipeWriter pipeWriter,
            CancellationTokenSource connectionClosedCts)
        {
            Stream = stream;
            _pipeWriter = pipeWriter;
            _connectionClosedCts = connectionClosedCts;
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

            _task = FillPipelineAsync(
                Stream,
                _pipeWriter,
                _connectionClosedCts,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _jobStopped.Cancel();

            return _task;
        }

        /// <inheritdoc />
        public Task PauseAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _jobPaused.Cancel();

            return _task;
        }

        /// <inheritdoc />
        public Task ContinueAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _jobPaused = new CancellationTokenSource();

            _task = FillPipelineAsync(
                Stream,
                _pipeWriter,
                _connectionClosedCts,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        [NotNull]
        private static async Task FillPipelineAsync(
            [NotNull] Stream stream,
            [NotNull] PipeWriter writer,
            CancellationTokenSource connectionClosedCts,
            CancellationToken jobStopped,
            CancellationToken jobPaused,
            IProgress<ConnectionStatus> statusProgress)
        {
            var globalCts = CancellationTokenSource.CreateLinkedTokenSource(connectionClosedCts.Token, jobStopped, jobPaused);
            var buffer = new byte[1024];

            statusProgress.Report(ConnectionStatus.Running);

            Exception exception = null;
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(buffer.Length);
                try
                {
                    var bytesRead = await stream
                       .ReadAsync(buffer, 0, buffer.Length, globalCts.Token)
                       .ConfigureAwait(false);
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
            writer.Complete(exception);

            // Signal a closed connection.
            connectionClosedCts.Cancel();

            // Change the status
            statusProgress.Report(ConnectionStatus.Stopped);
        }
    }
}
