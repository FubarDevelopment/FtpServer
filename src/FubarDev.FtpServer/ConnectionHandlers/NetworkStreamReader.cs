// <copyright file="NetworkStreamReader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Reads from a stream and writes into a pipeline.
    /// </summary>
    public class NetworkStreamReader : ICommunicationService
    {
        [NotNull]
        private readonly NetworkStream _stream;

        [NotNull]
        private readonly PipeWriter _pipeWriter;

        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        private readonly CancellationTokenSource _connectionClosedCts;

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
        /// <param name="connectionClosedCts">Cancellation token source for a closed connection.</param>
        /// <param name="logger">The logger.</param>
        public NetworkStreamReader(
            [NotNull] NetworkStream stream,
            [NotNull] PipeWriter pipeWriter,
            CancellationTokenSource connectionClosedCts,
            [CanBeNull] ILogger<NetworkStreamReader> logger = null)
        {
            _stream = stream;
            _pipeWriter = pipeWriter;
            _connectionClosedCts = connectionClosedCts;
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
                _connectionClosedCts,
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
                _connectionClosedCts,
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
            CancellationTokenSource connectionClosedCts,
            CancellationToken jobStopped,
            CancellationToken jobPaused,
            IProgress<ConnectionStatus> statusProgress,
            [CanBeNull] ILogger logger)
        {
            var globalCts = CancellationTokenSource.CreateLinkedTokenSource(connectionClosedCts.Token, jobStopped, jobPaused);

            statusProgress.Report(ConnectionStatus.Running);

            logger?.LogTrace("Starting filling pipeline");

            var networkStream = stream as NetworkStream;

            var buffer = new byte[1024];
            Exception exception = null;
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(buffer.Length);
                try
                {
                    logger?.LogTrace("Start reading with buffer size = {bufferSize}", buffer.Length);

                    int bytesRead;
                    if (networkStream != null)
                    {
                        // TODO: Find a better way.
                        // Maybe I have to resort to add another layer, which does a translation
                        // from PipeWriter to SslStream to PipeReader, because I can cancel the
                        // read operation of a pipe reader.
                        while (!networkStream.DataAvailable)
                        {
                            await Task.Delay(5, globalCts.Token).ConfigureAwait(false);
                        }

                        bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, globalCts.Token)
                           .ConfigureAwait(false);
                    }
                    else
                    {
                        var readTask = stream
                           .ReadAsync(buffer, 0, buffer.Length, globalCts.Token);

                        var resultTask = await Task.WhenAny(readTask, Task.Delay(-1, globalCts.Token))
                           .ConfigureAwait(false);
                        if (resultTask != readTask)
                        {
                            logger?.LogTrace("Cancelled through Task.Delay");
                            break;
                        }

                        bytesRead = readTask.Result;
                    }

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
                catch (Exception ex)
                {
                    // Log?.LogError(ex, ex.Message);
                    logger?.LogTrace(0, ex, ex.Message);
                    exception = ex;
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

            // Tell the PipeReader that there's no more data coming
            writer.Complete(exception);

            // Signal a closed connection.
            connectionClosedCts.Cancel();

            // Change the status
            statusProgress.Report(ConnectionStatus.Stopped);

            logger?.LogTrace("Stopped reader");
        }
    }
}
