// <copyright file="EnumerableExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Utilities
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to convert.</param>
        /// <returns>The converted <see cref="IEnumerable{T}"/>.</returns>
        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            return new EnumerableAsAsyncEnumerable<T>(enumerable);
        }

        /// <summary>
        /// Implementation of an <see cref="IAsyncEnumerable{T}"/> that redirects to an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        private class EnumerableAsAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _elements;

            /// <summary>
            /// Initializes a new instance of the <see cref="EnumerableAsAsyncEnumerable{T}"/> class.
            /// </summary>
            /// <param name="elements">The elements to be returned.</param>
            public EnumerableAsAsyncEnumerable(IEnumerable<T> elements)
            {
                _elements = elements;
            }

            /// <inheritdoc />
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new LinesEnumerator(_elements.GetEnumerator(), cancellationToken);
            }

            private class LinesEnumerator : IAsyncEnumerator<T>
            {
                private readonly IEnumerator<T> _enumerator;
                private readonly CancellationToken _cancellationToken;

                public LinesEnumerator(IEnumerator<T> enumerator, CancellationToken cancellationToken)
                {
                    _enumerator = enumerator;
                    _cancellationToken = cancellationToken;
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
                    _cancellationToken.ThrowIfCancellationRequested();
                    return new ValueTask<bool>(_enumerator.MoveNext());
                }
            }
        }
    }
}
