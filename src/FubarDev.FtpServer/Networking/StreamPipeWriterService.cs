// <copyright file="StreamPipeWriterService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Networking
{
    /// <summary>
    /// Reads from a pipe and writes to a stream.
    /// </summary>
    internal class StreamPipeWriterService : PausableFtpService
    {
        private readonly PipeReader _pipeReader;

        private Exception? _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamPipeWriterService"/> class.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="pipeReader">The pipe to read from.</param>
        /// <param name="connectionClosed">Cancellation token for a closed connection.</param>
        /// <param name="logger">The logger.</param>
        public StreamPipeWriterService(
            Stream stream,
            PipeReader pipeReader,
            CancellationToken connectionClosed,
            ILogger? logger = null)
            : base(connectionClosed, logger)
        {
            Stream = stream;
            _pipeReader = pipeReader;
        }

        /// <summary>
        /// Gets the stream used to write the output.
        /// </summary>
        protected Stream Stream { get; }

        /// <inheritdoc />
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken)
               .ConfigureAwait(false);
            await SafeFlushAsync(cancellationToken)
               .ConfigureAwait(false);
            await OnCloseAsync(_exception, cancellationToken)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                Logger?.LogTrace("Start reading response");
                var readResult = await _pipeReader.ReadAsync(cancellationToken)
                   .ConfigureAwait(false);

                try
                {
                    // Don't use the cancellation token source from above. Otherwise
                    // data might be lost.
                    await SendDataToStream(readResult.Buffer, CancellationToken.None)
                       .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger?.LogWarning(ex, "Sending data failed {ErrorMessage}", ex.Message);

                    // Ensure that the read operation is finished, but keep the data.
                    _pipeReader.AdvanceTo(readResult.Buffer.Start);
                    throw;
                }

                _pipeReader.AdvanceTo(readResult.Buffer.End);

                if (readResult.IsCanceled || readResult.IsCompleted)
                {
                    Logger?.LogTrace("Was cancelled or completed");
                    break;
                }
            }
        }

        /// <inheritdoc />
        protected override Task OnPausedAsync(CancellationToken cancellationToken)
        {
            return SafeFlushAsync(cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<bool> OnFailedAsync(Exception exception, CancellationToken cancellationToken)
        {
            // Error, but paused? Don't close the pipe!
            if (IsPauseRequested)
            {
                return false;
            }

            // Do whatever the base class wants to do.
            await base.OnFailedAsync(exception, cancellationToken)
               .ConfigureAwait(false);

            // Remember exception
            _exception = exception;

            return true;
        }

        /// <summary>
        /// Called when the stream is closed.
        /// </summary>
        /// <param name="exception">The exception that occurred during the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        protected virtual Task OnCloseAsync(Exception? exception, CancellationToken cancellationToken)
        {
            // Tell the PipeReader that there's no more data coming
            _pipeReader.Complete(exception);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="buffer">The buffer containing the data.</param>
        /// <param name="offset">The offset into the buffer.</param>
        /// <param name="length">The length of the data to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        protected virtual Task WriteToStreamAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return Stream.WriteAsync(buffer, offset, length, cancellationToken);
        }

        private async Task FlushAsync(
            CancellationToken cancellationToken)
        {
            Logger?.LogTrace("Flushing");
            while (!cancellationToken.IsCancellationRequested && _pipeReader.TryRead(out var readResult))
            {
                try
                {
                    await SendDataToStream(readResult.Buffer, CancellationToken.None)
                       .ConfigureAwait(false);
                }
                finally
                {
                    // Always advance to the end, because the data cannot
                    // be sent anyways.
                    _pipeReader.AdvanceTo(readResult.Buffer.End);
                }

                if (readResult.IsCanceled || readResult.IsCompleted)
                {
                    break;
                }
            }

            Logger?.LogTrace("Flushed");
        }

        private async Task SendDataToStream(
            ReadOnlySequence<byte> buffer,
            CancellationToken cancellationToken)
        {
            Logger?.LogTrace("Start sending");
            var position = buffer.Start;

            while (buffer.TryGet(ref position, out var memory))
            {
                var streamBuffer = memory.ToArray();
                await WriteToStreamAsync(streamBuffer, 0, streamBuffer.Length, cancellationToken)
                   .ConfigureAwait(false);
            }

            Logger?.LogTrace("Flushing stream");
            await Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SafeFlushAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex.Is<IOException>())
            {
                // Ignored. Connection closed by client?
            }
            catch (Exception ex) when (ex.Is<OperationCanceledException>())
            {
                // Ignored. Connection closed by server?
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(0, ex, "Flush failed with: {message}", ex.Message);
            }
        }
    }
}
