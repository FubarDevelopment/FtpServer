// <copyright file="RawStream.cs" company="Fubar Development Junker">
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
    /// A stream that uses a pipe.
    /// </summary>
    internal class RawStream : Stream
    {
        [NotNull]
        private readonly PipeReader _input;

        [NotNull]
        private readonly PipeWriter _output;

        [CanBeNull]
        private readonly ILogger<RawStream> _logger;

        public RawStream(
            [NotNull] PipeReader input,
            [NotNull] PipeWriter output,
            [CanBeNull] ILogger<RawStream> logger)
        {
            _input = input;
            _output = output;
            _logger = logger;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            _logger?.LogTrace("Try to read {count} bytes", count);

            // ValueTask uses .GetAwaiter().GetResult() if necessary
            // https://github.com/dotnet/corefx/blob/f9da3b4af08214764a51b2331f3595ffaf162abe/src/System.Threading.Tasks.Extensions/src/System/Threading/Tasks/ValueTask.cs#L156
            return ReadAsyncInternal(new Memory<byte>(buffer, offset, count), CancellationToken.None).Result;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _logger?.LogTrace("Try to read {count} bytes asynchronously", count);
            return ReadAsyncInternal(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _logger?.LogTrace("Try to write {count} bytes", count);
            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _logger?.LogTrace("Try to write {count} bytes asynchronously", count);

            if (buffer != null)
            {
                _output.Write(new ReadOnlySpan<byte>(buffer, offset, count));
            }

            await _output.FlushAsync(cancellationToken);
        }

        public override void Flush()
        {
            FlushAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return WriteAsync(null, 0, 0, cancellationToken);
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

                if (result.IsCompleted)
                {
                    return 0;
                }
            }
        }
    }
}
