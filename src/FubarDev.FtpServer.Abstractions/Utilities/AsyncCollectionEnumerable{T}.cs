// <copyright file="AsyncCollectionEnumerable{T}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Utilities
{
    /// <summary>
    /// Wraps an <see cref="IEnumerable{T}"/> of string in an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type of the enumeration.</typeparam>
    internal class AsyncCollectionEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _lines;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCollectionEnumerable{T}"/> class.
        /// </summary>
        /// <param name="lines">The lines to be returned.</param>
        public AsyncCollectionEnumerable(IEnumerable<T> lines)
        {
            _lines = lines;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerator(_lines.GetEnumerator());
        }

        private class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public AsyncEnumerator(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            /// <inheritdoc />
            public T Current => _enumerator.Current;

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                _enumerator.Dispose();
                return default;
            }

            /// <inheritdoc />
            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_enumerator.MoveNext());
            }
        }
    }
}
