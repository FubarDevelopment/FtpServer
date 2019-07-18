// <copyright file="PathSelector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Selects a path in a file system.
    /// </summary>
    public static class PathSelector
    {
        /// <summary>
        /// Tries to select the given <paramref name="path"/>.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to select.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the selection.</returns>
        public static Task<PathSelectionResult> SelectAsync(
            this IUnixFileSystem fileSystem,
            string? path,
            CancellationToken cancellationToken)
        {
            return SelectAsync(fileSystem, Enumerable.Empty<IUnixDirectoryEntry>(), path, cancellationToken);
        }

        /// <summary>
        /// Tries to select the given <paramref name="path"/>.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="pathEntries">The current path (entries).</param>
        /// <param name="path">The path to select.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the selection.</returns>
        public static async Task<PathSelectionResult> SelectAsync(
            this IUnixFileSystem fileSystem,
            IEnumerable<IUnixDirectoryEntry> pathEntries,
            string? path,
            CancellationToken cancellationToken)
        {
            var currentDirectoryEntries = new Stack<IUnixDirectoryEntry>(pathEntries);
            IUnixDirectoryEntry currentDirectoryEntry;
            if (currentDirectoryEntries.Count == 0)
            {
                currentDirectoryEntry = fileSystem.Root;
                currentDirectoryEntries.Push(currentDirectoryEntry);
            }
            else
            {
                currentDirectoryEntry = currentDirectoryEntries.Peek();
            }

            var pathSegments = new PathEnumerator(path)
               .NormalizePath(GetPathEntryNames(currentDirectoryEntries))
               .ToArray();
            for (var pathSegmentIndex = 0; pathSegmentIndex != pathSegments.Length; ++pathSegmentIndex)
            {
                var pathSegment = pathSegments[pathSegmentIndex];

                if (pathSegment == "/")
                {
                    currentDirectoryEntries.Clear();
                    currentDirectoryEntry = fileSystem.Root;
                    currentDirectoryEntries.Push(currentDirectoryEntry);
                    continue;
                }

                var isDirectoryExpected = pathSegment.EndsWith("/");
                var childName = isDirectoryExpected ? pathSegment.Substring(0, pathSegment.Length - 1) : pathSegment;
                var nextEntry = await fileSystem.GetEntryByNameAsync(
                        currentDirectoryEntry,
                        childName,
                        cancellationToken)
                   .ConfigureAwait(false);

                if (nextEntry == null)
                {
                    // Entry not found
                    var missingPathSegments = pathSegments.Skip(pathSegmentIndex).ToArray();
                    var lastIsDirectory = missingPathSegments[missingPathSegments.Length - 1].EndsWith("/");
                    if (lastIsDirectory)
                    {
                        return PathSelectionResult.CreateMissingDirectory(
                            currentDirectoryEntries,
                            missingPathSegments);
                    }

                    return PathSelectionResult.CreateMissingFileOrDirectory(
                        currentDirectoryEntries,
                        missingPathSegments);
                }

                var isDirectory = nextEntry is IUnixDirectoryEntry;
                if (isDirectoryExpected && !isDirectory)
                {
                    // File instead of directory found
                    var missingPathSegments = pathSegments.Skip(pathSegmentIndex).ToArray();
                    return PathSelectionResult.CreateMissingDirectory(
                        currentDirectoryEntries,
                        missingPathSegments);
                }

                if (!isDirectory)
                {
                    return PathSelectionResult.Create(
                        currentDirectoryEntries,
                        (IUnixFileEntry)nextEntry);
                }

                currentDirectoryEntry = (IUnixDirectoryEntry)nextEntry;
                currentDirectoryEntries.Push(currentDirectoryEntry);
            }

            // Found directory
            return PathSelectionResult.Create(
                currentDirectoryEntries);
        }
        private static IEnumerable<string> GetPathEntryNames(this Stack<IUnixDirectoryEntry> pathEntries)
        {
            return pathEntries.Reverse().GetPathEntryNames();
        }
        private static IEnumerable<string> GetPathEntryNames(this IEnumerable<IUnixDirectoryEntry> pathEntries)
        {
            return pathEntries.Select(pathEntry => pathEntry.GetPathSegment());
        }
    }
}
