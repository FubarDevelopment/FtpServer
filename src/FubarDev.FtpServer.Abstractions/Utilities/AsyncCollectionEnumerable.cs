// <copyright file="AsyncCollectionEnumerable.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace FubarDev.FtpServer.Utilities
{
    /// <summary>
    /// Static helpers for <see cref="AsyncCollectionEnumerable{T}"/>.
    /// </summary>
    public static class AsyncCollectionEnumerable
    {
        /// <summary>
        /// Gets an empty async enumeration of the given type.
        /// </summary>
        /// <typeparam name="T">Type of the empty enumeration.</typeparam>
        /// <returns>An empty enumeration.</returns>
        public static IAsyncEnumerable<T> Empty<T>() => AsyncEnumerableConstants<T>.EmptyEnum;

        /// <summary>
        /// Creates an async enumeration from a given enumeration.
        /// </summary>
        /// <typeparam name="T">Element type of the enumeration.</typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <returns>The async enumeration.</returns>
        public static IAsyncEnumerable<T> From<T>(IEnumerable<T> source)
        {
            return new AsyncCollectionEnumerable<T>(source);
        }

        private static class AsyncEnumerableConstants<T>
        {
            internal static IAsyncEnumerable<T> EmptyEnum { get; } = new AsyncCollectionEnumerable<T>(Enumerable.Empty<T>());
        }
    }
}
