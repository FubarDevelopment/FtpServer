// <copyright file="StreamPipeReaderService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Networking
{
    /// <summary>
    /// Reads from a stream and writes into a pipeline.
    /// </summary>
    internal class StreamPipeReaderService : PausableFtpService
    {
        private readonly PipeWriter _pipeWriter;

        private Exception? _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamPipeReaderService"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="pipeWriter">The pipeline to write to.</param>
        /// <param name="connectionClosed">Cancellation token for a closed connection.</param>
        /// <param name="logger">The logger.</param>
        public StreamPipeReaderService(
            Stream stream,
            PipeWriter pipeWriter,
            CancellationToken connectionClosed,
            ILogger? logger = null)
            : base(connectionClosed, logger)
        {
            Stream = stream;
            _pipeWriter = pipeWriter;
        }
        protected Stream Stream { get; }

        protected virtual Task<int> ReadFromStreamAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return Stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[32];
            while (!cancellationToken.IsCancellationRequested)
            {
                // Allocate at least 512 bytes from the PipeWriter
                var memory = _pipeWriter.GetMemory(buffer.Length);

                Logger?.LogTrace("Start reading from stream");
                var bytesRead = await ReadFromStreamAsync(buffer, 0, buffer.Length, cancellationToken)
                   .ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    Logger?.LogTrace("Stream closed");
                    break;
                }

                Logger?.LogTrace("Copied {numBytes} bytes into pipe", bytesRead);
                buffer.AsSpan(0, bytesRead).CopyTo(memory.Span);

                // Tell the PipeWriter how much was read from the Socket
                _pipeWriter.Advance(bytesRead);

                // Make the data available to the PipeReader.
                // Don't use the cancellation token source from above. Otherwise
                // data might be lost.
                var result = await _pipeWriter.FlushAsync(CancellationToken.None);
                if (result.IsCompleted || result.IsCanceled)
                {
                    Logger?.LogTrace("Writer completed or cancelled");
                    break;
                }
            }
        }

        /// <inheritdoc />
        protected override Task OnStoppedAsync(CancellationToken cancellationToken)
        {
            return OnCloseAsync(_exception, cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<bool> OnFailedAsync(Exception exception, CancellationToken cancellationToken)
        {
            // Error, but paused? Don't close the pipe!
            if (IsPauseRequested)
            {
                return true;
            }

            // Do whatever the base class wants to do.
            await base.OnFailedAsync(exception, cancellationToken)
               .ConfigureAwait(false);

            _exception = exception;

            return true;
        }

        protected virtual Task OnCloseAsync(Exception? exception, CancellationToken cancellationToken)
        {
            // Tell the PipeReader that there's no more data coming
            _pipeWriter.Complete(exception);

            return Task.CompletedTask;
        }
    }
}
