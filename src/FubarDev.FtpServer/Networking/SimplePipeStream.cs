// <copyright file="SimplePipeStream.cs" company="Fubar Development Junker">
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
    /// A stream that uses a pipe.
    /// </summary>
    internal class SimplePipeStream : Stream
    {
        private readonly PipeReader _input;
        private readonly PipeWriter _output;

        private readonly ILogger<SimplePipeStream>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePipeStream"/> class.
        /// </summary>
        /// <param name="input">The pipe reader to be used to read from.</param>
        /// <param name="output">The pipe writer to be used to write to.</param>
        /// <param name="logger">The logger.</param>
        public SimplePipeStream(
            PipeReader input,
            PipeWriter output,
            ILogger<SimplePipeStream>? logger = null)
        {
            _input = input;
            _output = output;
            _logger = logger;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            _logger?.LogTrace("Try to read {count} bytes", count);

            // ValueTask uses .GetAwaiter().GetResult() if necessary
            // https://github.com/dotnet/corefx/blob/f9da3b4af08214764a51b2331f3595ffaf162abe/src/System.Threading.Tasks.Extensions/src/System/Threading/Tasks/ValueTask.cs#L156
            return ReadAsyncInternal(new Memory<byte>(buffer, offset, count), CancellationToken.None).Result;
        }

        /// <inheritdoc />
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _logger?.LogTrace("Try to read {count} bytes asynchronously", count);
            return ReadAsyncInternal(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _logger?.LogTrace("Try to write {count} bytes", count);
            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _logger?.LogTrace("Try to write {count} bytes asynchronously", count);

            if (buffer != null)
            {
                _output.Write(new ReadOnlySpan<byte>(buffer, offset, count));
            }

            await _output.FlushAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            FlushAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await _output.FlushAsync(cancellationToken);
        }

        private async ValueTask<int> ReadAsyncInternal(Memory<byte> destination, CancellationToken cancellationToken)
        {
            while (true)
            {
                var result = await _input.ReadAsync(cancellationToken)
                   .ConfigureAwait(false);

                var readableBuffer = result.Buffer;
                if (!readableBuffer.IsEmpty)
                {
                    _logger?.LogTrace("Received {byteCount} bytes", readableBuffer.Length);

                    var count = (int)Math.Min(readableBuffer.Length, destination.Length);
                    readableBuffer = readableBuffer.Slice(0, count);
                    readableBuffer.CopyTo(destination.Span);
                    _input.AdvanceTo(readableBuffer.GetPosition(count));

                    return count;
                }

                _input.AdvanceTo(readableBuffer.End);

                if (result.IsCompleted || result.IsCanceled)
                {
                    return 0;
                }
            }
        }
    }
}
