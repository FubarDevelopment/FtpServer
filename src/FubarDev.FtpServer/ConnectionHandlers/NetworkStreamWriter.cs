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

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Reads from a pipe and writes to a stream.
    /// </summary>
    internal class NetworkStreamWriter : CommunicationServiceBase
    {
        [NotNull]
        private readonly Stream _stream;

        [NotNull]
        private readonly PipeReader _pipeReader;

        [CanBeNull]
        private Exception _exception;

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
            [CanBeNull] ILogger logger = null)
            : base(connectionClosed, logger)
        {
            _stream = stream;
            _pipeReader = pipeReader;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                Logger?.LogTrace("Start reading response");
                var readResult = await _pipeReader.ReadAsync(cancellationToken)
                   .ConfigureAwait(false);

                // Don't use the cancellation token source from above. Otherwise
                // data might be lost.
                await SendDataToStream(readResult.Buffer, _stream, CancellationToken.None, Logger)
                   .ConfigureAwait(false);

                _pipeReader.AdvanceTo(readResult.Buffer.End);

                if (readResult.IsCanceled || readResult.IsCompleted)
                {
                    Logger?.LogTrace("Was cancelled or completed.");
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
        protected override async Task OnStoppedAsync(CancellationToken cancellationToken)
        {
            await SafeFlushAsync(cancellationToken)
               .ConfigureAwait(false);
            await OnCloseAsync(_exception, cancellationToken)
               .ConfigureAwait(false);
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

        protected virtual Task OnCloseAsync(Exception exception, CancellationToken cancellationToken)
        {
            // Tell the PipeReader that there's no more data coming
            _pipeReader.Complete(_exception);

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

                if (readResult.IsCanceled || readResult.IsCompleted)
                {
                    break;
                }
            }

            logger?.LogTrace("Flushed");
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

        private async Task SafeFlushAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await FlushAsync(_stream, _pipeReader, cancellationToken, Logger).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex.IsIOException())
            {
                // Ignored. Connection closed by client?
            }
            catch (Exception ex) when (ex.IsOperationCancelledException())
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
