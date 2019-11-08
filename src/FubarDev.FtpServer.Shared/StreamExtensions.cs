// <copyright file="StreamExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="Stream"/>.
    /// </summary>
    internal static class StreamExtensions
    {
        /// <summary>
        /// Copy to target stream, while flushing the data after every operation.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destination">The destination stream.</param>
        /// <param name="bufferSize">The copy buffer size.</param>
        /// <param name="flush">Indicates whether the data should be flushed after every operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, bool flush, CancellationToken cancellationToken)
        {
            if (!flush)
            {
                await source.CopyToAsync(destination, bufferSize, cancellationToken);
                return;
            }

            var buffer = new byte[bufferSize];
            int readCount;
            while ((readCount = await source.ReadAsync(buffer, 0, bufferSize, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, readCount, cancellationToken).ConfigureAwait(false);
                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
