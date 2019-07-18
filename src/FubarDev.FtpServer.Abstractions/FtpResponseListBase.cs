// <copyright file="FtpResponseListBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An FTP response for lists.
    /// </summary>
    public abstract class FtpResponseListBase : IFtpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseListBase"/> class.
        /// </summary>
        /// <param name="code">The status code.</param>
        protected FtpResponseListBase(
            int code)
        {
            Code = code;
        }

        /// <inheritdoc />
        public int Code { get; }

        /// <inheritdoc />
        [Obsolete("Use a custom server command.")]
        public FtpResponseAfterWriteAsyncDelegate? AfterWriteAction => null;

        /// <inheritdoc />
        public IAsyncEnumerable<string> GetNextLineAsync(CancellationToken cancellationToken)
        {
            return new LinesEnumerable(GetOutputLines(cancellationToken));
        }

        /// <summary>
        /// Gets the output lines for all source lines.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The lines to send to the client.</returns>
        protected abstract IEnumerable<string> GetOutputLines(CancellationToken cancellationToken);

        private class LinesEnumerable : IAsyncEnumerable<string>
        {
            private readonly IEnumerable<string> _lines;

            public LinesEnumerable(IEnumerable<string> lines)
            {
                _lines = lines;
            }

            /// <inheritdoc />
            public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new LinesEnumerator(_lines.GetEnumerator());
            }

            private class LinesEnumerator : IAsyncEnumerator<string>
            {
                private readonly IEnumerator<string> _linesEnumerator;

                public LinesEnumerator(IEnumerator<string> linesEnumerator)
                {
                    _linesEnumerator = linesEnumerator;
                }

                /// <inheritdoc />
                public string Current => _linesEnumerator.Current;

                /// <inheritdoc />
                public ValueTask DisposeAsync()
                {
                    _linesEnumerator.Dispose();
                    return default;
                }

                /// <inheritdoc />
                public ValueTask<bool> MoveNextAsync()
                {
                    return new ValueTask<bool>(_linesEnumerator.MoveNext());
                }
            }
        }
    }
}
