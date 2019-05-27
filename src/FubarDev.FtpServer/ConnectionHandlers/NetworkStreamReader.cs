// <copyright file="NetworkStreamReader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Reads from a stream and writes into a pipeline.
    /// </summary>
    public class NetworkStreamReader : CommunicationServiceBase
    {
        [NotNull]
        private readonly Stream _stream;

        [NotNull]
        private readonly PipeWriter _pipeWriter;

        [CanBeNull]
        private Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkStreamReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="pipeWriter">The pipeline to write to.</param>
        /// <param name="connectionClosed">Cancellation token for a closed connection.</param>
        /// <param name="logger">The logger.</param>
        public NetworkStreamReader(
            [NotNull] Stream stream,
            [NotNull] PipeWriter pipeWriter,
            CancellationToken connectionClosed,
            [CanBeNull] ILogger logger = null)
            : base(connectionClosed, logger)
        {
            _stream = stream;
            _pipeWriter = pipeWriter;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                var memory = _pipeWriter.GetMemory(buffer.Length);
                var readTask = _stream
                   .ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                var resultTask = await Task.WhenAny(readTask, Task.Delay(-1, cancellationToken))
                   .ConfigureAwait(false);
                if (resultTask != readTask)
                {
                    Logger?.LogTrace("Cancelled through Task.Delay");
                    break;
                }

                var bytesRead = readTask.Result;

                if (bytesRead == 0)
                {
                    break;
                }

                buffer.AsSpan(0, bytesRead).CopyTo(memory.Span);

                // Tell the PipeWriter how much was read from the Socket
                _pipeWriter.Advance(bytesRead);

                // Make the data available to the PipeReader.
                // Don't use the cancellation token source from above. Otherwise
                // data might be lost.
                var result = await _pipeWriter.FlushAsync(CancellationToken.None);
                if (result.IsCompleted || result.IsCanceled)
                {
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

        protected virtual Task OnCloseAsync(Exception exception, CancellationToken cancellationToken)
        {
            // Tell the PipeReader that there's no more data coming
            _pipeWriter.Complete(_exception);

            return Task.CompletedTask;
        }
    }
}
