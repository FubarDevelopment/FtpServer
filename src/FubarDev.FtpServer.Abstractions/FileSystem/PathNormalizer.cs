// <copyright file="PathNormalizer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Path normalization by removing its <c>.</c> and <c>..</c> segments and replacing <c>\\</c> with <c>/</c>.
    /// </summary>
    public static class PathNormalizer
    {
        /// <summary>
        /// Normalize a path by removing its <c>.</c> and <c>..</c> segments.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized path.</returns>
        public static string NormalizePath(this string path)
        {
            return string.Join(string.Empty, new PathEnumerator(path).NormalizePath());
        }

        /// <summary>
        /// Normalize a path by removing its <c>.</c> and <c>..</c> segments.
        /// </summary>
        /// <param name="pathSegments">The segments of the path to normalize.</param>
        /// <param name="previousPathSegments">Initial path segments to be used as reference.</param>
        /// <returns>The normalized path segments.</returns>
        public static IEnumerable<string> NormalizePath(
            this IEnumerable<string> pathSegments,
            IEnumerable<string>? previousPathSegments = null)
        {
            var initialEntries = previousPathSegments?.ToArray() ?? Array.Empty<string>();
            var entries = new Stack<string>(initialEntries);
            var minElements = initialEntries.Length != 0 && initialEntries[0] == "/" ? 1 : 0;
            foreach (var pathSegment in pathSegments)
            {
                switch (pathSegment)
                {
                    case "\\":
                    case "/":
                        // Root entry
                        entries.Clear();
                        entries.Push("/");
                        minElements = 1;
                        break;

                    case ".":
                    case "./":
                    case ".\\":
                        // Current directory
                        break;

                    case "../":
                    case "..\\":
                    case "..":
                        // Parent directory
                        if (entries.Count > minElements)
                        {
                            entries.Pop();
                        }

                        break;

                    default:
                        if (pathSegment.EndsWith("\\"))
                        {
                            entries.Push(pathSegment.Substring(0, pathSegment.Length - 1) + "/");
                        }
                        else
                        {
                            entries.Push(pathSegment);
                        }

                        break;
                }
            }

            return entries.Reverse();
        }
        internal static string GetPathSegment(this IUnixDirectoryEntry entry)
        {
            if (entry.IsRoot)
            {
                return "/";
            }

            return entry.Name + "/";
        }
    }
}
