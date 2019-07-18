// <copyright file="PathEnumerator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Enumerates the parts of a path.
    /// </summary>
    public class PathEnumerator : IEnumerable<string>
    {
        private readonly string? _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathEnumerator"/> class.
        /// </summary>
        /// <param name="path">The path to enumerate.</param>
        public PathEnumerator(string? path)
        {
            _path = path;
        }

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
        {
            if (string.IsNullOrEmpty(_path))
            {
                return Enumerable.Empty<string>().GetEnumerator();
            }

            return SplitPath(_path).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        private static IEnumerable<string> SplitPath(string path)
        {
            var lastIndex = 0;
            var indexOfSlash = path.IndexOf('/');
            while (indexOfSlash != -1)
            {
                yield return path.Substring(lastIndex, indexOfSlash - lastIndex + 1);
                lastIndex = indexOfSlash + 1;
                indexOfSlash = path.IndexOf('/', lastIndex);
            }

            var remaining = path.Substring(lastIndex);
            if (!string.IsNullOrEmpty(remaining))
            {
                yield return remaining;
            }
        }
    }
}
