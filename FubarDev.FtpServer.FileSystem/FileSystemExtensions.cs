//-----------------------------------------------------------------------
// <copyright file="FileSystemExtensions.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Extensions for the file system stuff
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Is this directory a root directory?
        /// </summary>
        /// <param name="directoryEntry">The directory entry to test</param>
        /// <returns><code>true</code> if the <paramref name="directoryEntry"/> is a root directory</returns>
        public static bool IsRoot([NotNull] this IUnixDirectoryEntry directoryEntry)
        {
            return string.IsNullOrEmpty(directoryEntry.Name);
        }

        /// <summary>
        /// Clone the stack of directory entries
        /// </summary>
        /// <param name="path">The stack of directory entries to clone</param>
        /// <returns>the cloned <paramref name="path"/></returns>
        public static Stack<IUnixDirectoryEntry> Clone(this Stack<IUnixDirectoryEntry> path)
        {
            return new Stack<IUnixDirectoryEntry>(path.Reverse());
        }

        /// <summary>
        /// Is the <paramref name="pathToTestAsChild"/> a child or the same path as <paramref name="pathToTestAsParent"/>?
        /// </summary>
        /// <param name="pathToTestAsChild">The path to test as child</param>
        /// <param name="pathToTestAsParent">The path to test as parent</param>
        /// <param name="fileSystem">The file system to use to compare the file names</param>
        /// <returns><code>true</code> if the <paramref name="pathToTestAsChild"/> is a child or the same path as <paramref name="pathToTestAsParent"/></returns>
        public static bool IsChildOfOrSameAs(this Stack<IUnixDirectoryEntry> pathToTestAsChild, Stack<IUnixDirectoryEntry> pathToTestAsParent, IUnixFileSystem fileSystem)
        {
            var fullPathOfParent = pathToTestAsParent.GetFullPath();
            var fullPathOfChild = pathToTestAsChild.GetFullPath();
            var testPathOfChild = fullPathOfChild.Substring(0, Math.Min(fullPathOfChild.Length, fullPathOfChild.Length));
            return fileSystem.FileSystemEntryComparer.Equals(testPathOfChild, fullPathOfParent) && fullPathOfParent.Length <= fullPathOfChild.Length;
        }

        /// <summary>
        /// Get the directory for the given <paramref name="path"/>
        /// </summary>
        /// <param name="fileSystem">The file system to get the directory for</param>
        /// <param name="currentPath">The current path</param>
        /// <param name="path">The (absolute or relative) path to get the directory for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/> or <code>null</code></returns>
        public static Task<IUnixDirectoryEntry> GetDirectoryAsync(this IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> currentPath, string path, CancellationToken cancellationToken)
        {
            var pathElements = GetPathElements(path);
            return GetDirectoryAsync(fileSystem, currentPath, pathElements, cancellationToken);
        }

        /// <summary>
        /// Get the directory for the given <paramref name="pathElements"/>
        /// </summary>
        /// <param name="fileSystem">The file system to get the directory for</param>
        /// <param name="currentPath">The current path</param>
        /// <param name="pathElements">The (absolute or relative) path to get the directory for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/> or <code>null</code></returns>
        public static async Task<IUnixDirectoryEntry> GetDirectoryAsync(this IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> currentPath, IReadOnlyList<string> pathElements, CancellationToken cancellationToken)
        {
            IUnixDirectoryEntry currentDir = currentPath.Count == 0 ? fileSystem.Root : currentPath.Peek();
            foreach (var pathElement in pathElements)
            {
                if (pathElement == string.Empty)
                {
                    currentDir = fileSystem.Root;
                    currentPath.Clear();
                    continue;
                }
                if (pathElement == ".")
                    continue;
                if (pathElement == "..")
                {
                    if (currentPath.Count != 0)
                        currentPath.Pop();
                    continue;
                }
                var foundEntry = (IUnixDirectoryEntry)await fileSystem.GetEntryByNameAsync(currentDir, pathElement, cancellationToken);
                if (foundEntry == null)
                    return null;
                currentPath.Push(foundEntry);
                currentDir = foundEntry;
            }
            return currentDir;
        }

        public static Task<SearchResult<IUnixDirectoryEntry>> SearchDirectoryAsync(this IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> currentPath, string path, CancellationToken cancellationToken)
        {
            var pathElements = GetPathElements(path);
            return SearchDirectoryAsync(fileSystem, currentPath, pathElements, cancellationToken);
        }

        public static async Task<SearchResult<IUnixDirectoryEntry>> SearchDirectoryAsync(this IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> currentPath, IReadOnlyList<string> pathElements, CancellationToken cancellationToken)
        {
            var sourceDir = await GetDirectoryAsync(fileSystem, currentPath, new ListSegment<string>(pathElements, 0, pathElements.Count - 1), cancellationToken);
            if (sourceDir == null)
                return null;
            var fileName = pathElements[pathElements.Count - 1];
            var foundEntry = (IUnixDirectoryEntry)await fileSystem.GetEntryByNameAsync(sourceDir, fileName, cancellationToken);
            return new SearchResult<IUnixDirectoryEntry>(sourceDir, foundEntry, fileName);
        }

        public static Task<SearchResult<IUnixFileEntry>> SearchFileAsync(this IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> currentPath, string path, CancellationToken cancellationToken)
        {
            var pathElements = GetPathElements(path);
            return SearchFileAsync(fileSystem, currentPath, pathElements, cancellationToken);
        }

        public static async Task<SearchResult<IUnixFileEntry>> SearchFileAsync(this IUnixFileSystem fileSystem, Stack<IUnixDirectoryEntry> currentPath, IReadOnlyList<string> pathElements, CancellationToken cancellationToken)
        {
            var sourceDir = await GetDirectoryAsync(fileSystem, currentPath, new ListSegment<string>(pathElements, 0, pathElements.Count - 1), cancellationToken);
            if (sourceDir == null)
                return null;
            var fileName = pathElements[pathElements.Count - 1];
            var foundEntry = (IUnixFileEntry)await fileSystem.GetEntryByNameAsync(sourceDir, fileName, cancellationToken);
            return new SearchResult<IUnixFileEntry>(sourceDir, foundEntry, fileName);
        }

        public static string GetFullPath(this Stack<IUnixDirectoryEntry> path)
        {
            var result = new StringBuilder("/");
            foreach (var pathEntry in path.Reverse())
            {
                result.Append($"{pathEntry.Name}/");
            }
            return result.ToString();
        }

        public static string GetFullPath(this Stack<IUnixDirectoryEntry> path, string fileName)
        {
            var fullName = $"{path.GetFullPath()}{fileName}";
            return fullName;
        }

        /// <summary>
        /// Appends parts to a path
        /// </summary>
        /// <param name="path">The path to append to</param>
        /// <param name="parts">The parts to append</param>
        /// <returns>The </returns>
        public static string CombinePath(string path, params string[] parts)
        {
            return CombinePath(path, (IEnumerable<string>)parts);
        }

        /// <summary>
        /// Appends parts to a path
        /// </summary>
        /// <param name="path">The path to append to</param>
        /// <param name="parts">The parts to append</param>
        /// <returns>The </returns>
        public static string CombinePath(string path, IEnumerable<string> parts)
        {
            var result = new StringBuilder();
            bool addSlash;
            if (!string.IsNullOrEmpty(path))
            {
                result.Append(path);
                addSlash = !path.EndsWith("/");
            }
            else
            {
                addSlash = false;
            }
            foreach (var part in parts)
            {
                if (addSlash)
                    result.Append('/');
                else
                    addSlash = true;
                result.Append(part.Replace("\\", "\\\\").Replace("/", "\\/"));
            }
            return result.ToString();
        }

        /// <summary>
        /// Split the path into parts
        /// </summary>
        /// <param name="path">The path to split</param>
        /// <returns>The parts of the path</returns>
        public static IReadOnlyList<string> SplitPath(string path)
        {
            var parts = new List<string>();
            if (string.IsNullOrEmpty(path))
                return parts;
            var isEscaped = false;
            var partCollector = new StringBuilder();
            foreach (var ch in path.ToCharArray())
            {
                if (!isEscaped)
                {
                    if (ch == '\\')
                    {
                        isEscaped = true;
                    }
                    else if (ch == '/')
                    {
                        parts.Add(partCollector.ToString());
                        partCollector.Clear();
                    }
                    else
                    {
                        partCollector.Append(ch);
                    }
                }
                else
                {
                    if (ch == '/' || ch == '\\')
                    {
                        partCollector.Append(ch);
                    }
                    else
                    {
                        partCollector.Append('\\').Append(ch);
                    }
                    isEscaped = false;
                }
            }
            parts.Add(partCollector.ToString());
            return parts;
        }

        private static IReadOnlyList<string> GetPathElements(string path)
        {
            var pathElements = new List<string>();
            if (path.StartsWith("/"))
                pathElements.Add(string.Empty);
            var parts = SplitPath(path).Where(x => !string.IsNullOrEmpty(x));
            pathElements.AddRange(parts);
            return pathElements;
        }
    }
}
