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
    /// Extensions for the file system stuff.
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Clone the stack of directory entries.
        /// </summary>
        /// <param name="path">The stack of directory entries to clone.</param>
        /// <returns>the cloned <paramref name="path"/>.</returns>
        [NotNull]
        [ItemNotNull]
        public static Stack<IUnixDirectoryEntry> Clone([NotNull, ItemNotNull] this Stack<IUnixDirectoryEntry> path)
        {
            return new Stack<IUnixDirectoryEntry>(path.Reverse());
        }

        /// <summary>
        /// Determines whether the <paramref name="pathToTestAsChild"/> is a child or the same path as <paramref name="pathToTestAsParent"/>.
        /// </summary>
        /// <param name="pathToTestAsChild">The path to test as child.</param>
        /// <param name="pathToTestAsParent">The path to test as parent.</param>
        /// <param name="fileSystem">The file system to use to compare the file names.</param>
        /// <returns><code>true</code> if the <paramref name="pathToTestAsChild"/> is a child or the same path as <paramref name="pathToTestAsParent"/>.</returns>
        public static bool IsChildOfOrSameAs([NotNull, ItemNotNull] this Stack<IUnixDirectoryEntry> pathToTestAsChild, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> pathToTestAsParent, [NotNull] IUnixFileSystem fileSystem)
        {
            var fullPathOfParent = pathToTestAsParent.GetFullPath();
            var fullPathOfChild = pathToTestAsChild.GetFullPath();
            var testPathOfChild = fullPathOfChild.Substring(0, Math.Min(fullPathOfParent.Length, fullPathOfChild.Length));
            return fileSystem.FileSystemEntryComparer.Equals(testPathOfChild, fullPathOfParent) && fullPathOfParent.Length <= fullPathOfChild.Length;
        }

        /// <summary>
        /// Get the directory for the given <paramref name="path"/>.
        /// </summary>
        /// <param name="fileSystem">The file system to get the directory for.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="path">The (absolute or relative) path to get the directory for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/> or <c>null</c>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static Task<IUnixDirectoryEntry> GetDirectoryAsync([NotNull] this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [CanBeNull] string path, CancellationToken cancellationToken)
        {
            var pathElements = GetPathElements(path);
            return GetDirectoryAsync(fileSystem, currentPath, pathElements, cancellationToken);
        }

        /// <summary>
        /// Get the directory for the given <paramref name="pathElements"/>.
        /// </summary>
        /// <param name="fileSystem">The file system to get the directory for.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="pathElements">The (absolute or relative) path to get the directory for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/> or <c>null</c>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static async Task<IUnixDirectoryEntry> GetDirectoryAsync([NotNull] this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [NotNull, ItemNotNull] IReadOnlyList<string> pathElements, CancellationToken cancellationToken)
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
                {
                    continue;
                }

                if (pathElement == "..")
                {
                    if (currentPath.Count != 0)
                    {
                        currentPath.Pop();
                        currentDir = currentPath.Count == 0 ? fileSystem.Root : currentPath.Peek();
                    }
                    continue;
                }

                var foundEntry = await fileSystem.GetEntryByNameAsync(currentDir, pathElement, cancellationToken).ConfigureAwait(false);
                if (!(foundEntry is IUnixDirectoryEntry foundDirEntry))
                {
                    return null;
                }

                currentPath.Push(foundDirEntry);
                currentDir = foundDirEntry;
            }

            return currentDir;
        }

        /// <summary>
        /// Searches for a <see cref="IUnixDirectoryEntry"/> by using the <paramref name="currentPath"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="fileSystem">The underlying <see cref="IUnixFileSystem"/>.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="path">The relative path to search for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static Task<SearchResult<IUnixDirectoryEntry>> SearchDirectoryAsync([NotNull] this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [CanBeNull] string path, CancellationToken cancellationToken)
        {
            var pathElements = GetPathElements(path);
            return SearchDirectoryAsync(fileSystem, currentPath, pathElements, cancellationToken);
        }

        /// <summary>
        /// Searches for a <see cref="IUnixDirectoryEntry"/> by using the <paramref name="currentPath"/> and <paramref name="pathElements"/>.
        /// </summary>
        /// <param name="fileSystem">The underlying <see cref="IUnixFileSystem"/>.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="pathElements">The relative path elements to search for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static async Task<SearchResult<IUnixDirectoryEntry>> SearchDirectoryAsync([NotNull] this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [NotNull, ItemNotNull] IReadOnlyList<string> pathElements, CancellationToken cancellationToken)
        {
            var sourceDir = await GetDirectoryAsync(fileSystem, currentPath, new ListSegment<string>(pathElements, 0, pathElements.Count - 1), cancellationToken).ConfigureAwait(false);
            if (sourceDir == null)
            {
                return null;
            }

            var fileName = pathElements[pathElements.Count - 1];
            var foundEntry = await fileSystem.GetEntryByNameAsync(sourceDir, fileName, cancellationToken).ConfigureAwait(false);
            var foundDirEntry = foundEntry as IUnixDirectoryEntry;
            if (foundDirEntry == null && foundEntry != null)
            {
                return null;
            }

            return new SearchResult<IUnixDirectoryEntry>(sourceDir, foundDirEntry, fileName);
        }

        /// <summary>
        /// Searches for a <see cref="IUnixFileEntry"/> by using the <paramref name="currentPath"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="fileSystem">The underlying <see cref="IUnixFileSystem"/>.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="path">The relative path to search for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static Task<SearchResult<IUnixFileEntry>> SearchFileAsync([NotNull] this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [CanBeNull] string path, CancellationToken cancellationToken)
        {
            var pathElements = GetPathElements(path);
            return SearchFileAsync(fileSystem, currentPath, pathElements, cancellationToken);
        }

        /// <summary>
        /// Searches for a <see cref="IUnixFileEntry"/> by using the <paramref name="currentPath"/> and <paramref name="pathElements"/>.
        /// </summary>
        /// <param name="fileSystem">The underlying <see cref="IUnixFileSystem"/>.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="pathElements">The relative path elements to search for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixDirectoryEntry"/>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static async Task<SearchResult<IUnixFileEntry>> SearchFileAsync(this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [NotNull, ItemNotNull] IReadOnlyList<string> pathElements, CancellationToken cancellationToken)
        {
            var sourceDir = await GetDirectoryAsync(fileSystem, currentPath, new ListSegment<string>(pathElements, 0, pathElements.Count - 1), cancellationToken).ConfigureAwait(false);
            if (sourceDir == null)
            {
                return null;
            }

            var fileName = pathElements[pathElements.Count - 1];
            var foundEntry = await fileSystem.GetEntryByNameAsync(sourceDir, fileName, cancellationToken).ConfigureAwait(false);
            var foundFileEntry = foundEntry as IUnixFileEntry;
            if (foundEntry != null && foundFileEntry == null)
            {
                return null;
            }

            return new SearchResult<IUnixFileEntry>(sourceDir, foundFileEntry, fileName);
        }

        /// <summary>
        /// Searches for a <see cref="IUnixFileSystemEntry"/> by using the <paramref name="currentPath"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="fileSystem">The underlying <see cref="IUnixFileSystem"/>.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="path">The relative path to search for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixFileSystemEntry"/>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static Task<SearchResult<IUnixFileSystemEntry>> SearchEntryAsync([NotNull] this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [CanBeNull] string path, CancellationToken cancellationToken)
        {
            var pathElements = GetPathElements(path);
            return SearchEntryAsync(fileSystem, currentPath, pathElements, cancellationToken);
        }

        /// <summary>
        /// Searches for a <see cref="IUnixFileSystemEntry"/> by using the <paramref name="currentPath"/> and <paramref name="pathElements"/>.
        /// </summary>
        /// <param name="fileSystem">The underlying <see cref="IUnixFileSystem"/>.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="pathElements">The relative path elements to search for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found <see cref="IUnixFileSystemEntry"/>.</returns>
        [NotNull]
        [ItemCanBeNull]
        public static async Task<SearchResult<IUnixFileSystemEntry>> SearchEntryAsync(this IUnixFileSystem fileSystem, [NotNull, ItemNotNull] Stack<IUnixDirectoryEntry> currentPath, [NotNull, ItemNotNull] IReadOnlyList<string> pathElements, CancellationToken cancellationToken)
        {
            var sourceDir = await GetDirectoryAsync(fileSystem, currentPath, new ListSegment<string>(pathElements, 0, pathElements.Count - 1), cancellationToken).ConfigureAwait(false);
            if (sourceDir == null)
            {
                return null;
            }

            IUnixFileSystemEntry foundEntry;
            var fileName = pathElements[pathElements.Count - 1];
            switch (fileName)
            {
                case "":
                    currentPath.Clear();
                    fileName = null;
                    foundEntry = fileSystem.Root;
                    break;
                case ".":
                    if (currentPath.Count != 0)
                    {
                        foundEntry = currentPath.Pop();
                        fileName = foundEntry.Name;
                    }
                    else
                    {
                        foundEntry = fileSystem.Root;
                        fileName = null;
                    }

                    break;
                case "..":
                    if (currentPath.Count != 0)
                    {
                        currentPath.Pop();
                    }

                    if (currentPath.Count != 0)
                    {
                        foundEntry = currentPath.Pop();
                        fileName = foundEntry.Name;
                    }
                    else
                    {
                        foundEntry = fileSystem.Root;
                        fileName = null;
                    }

                    break;
                default:
                    foundEntry = await fileSystem.GetEntryByNameAsync(sourceDir, fileName, cancellationToken).ConfigureAwait(false);
                    break;
            }

            return new SearchResult<IUnixFileSystemEntry>(sourceDir, foundEntry, fileName);
        }

        /// <summary>
        /// Returns the <paramref name="path"/> as string like <see cref="GetFullPath(System.Collections.Generic.Stack{FubarDev.FtpServer.FileSystem.IUnixDirectoryEntry})"/>, with the
        /// difference that it doesn't add the trailing <c>/</c>.
        /// </summary>
        /// <param name="path">The path to convert to string.</param>
        /// <returns>The <paramref name="path"/> as string.</returns>
        [NotNull]
        public static string ToDisplayString([NotNull, ItemNotNull] this Stack<IUnixDirectoryEntry> path)
        {
            return $"/{string.Join("/", path.Reverse().Select(x => x.Name))}";
        }

        /// <summary>
        /// Returns the <paramref name="path"/> as string.
        /// </summary>
        /// <param name="path">The path to convert to string.</param>
        /// <returns>The <paramref name="path"/> as string.</returns>
        [NotNull]
        public static string GetFullPath([NotNull, ItemNotNull] this Stack<IUnixDirectoryEntry> path)
        {
            var result = new StringBuilder("/");
            foreach (var pathEntry in path.Reverse())
            {
                result.Append($"{pathEntry.Name}/");
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns the parent path of the <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to get the parent path from.</param>
        /// <returns>The parent path.</returns>
        [NotNull]
        public static string GetParentPath([NotNull] this string path)
        {
            var parts = GetPathElements(path);
            if (parts.Count == 0)
            {
                return path;
            }

            if (parts.Count == 1 && string.IsNullOrEmpty(parts[0]))
            {
                return path;
            }

            return CombinePath(null, parts.Take(parts.Count - 1));
        }

        /// <summary>
        /// Returns the <paramref name="path"/> as string.
        /// </summary>
        /// <param name="path">The path to convert to string.</param>
        /// <param name="fileName">The file name to append to the <paramref name="path"/>.</param>
        /// <returns>The combination of <paramref name="path"/> and <paramref name="fileName"/> as string.</returns>
        [NotNull]
        public static string GetFullPath([NotNull, ItemNotNull] this Stack<IUnixDirectoryEntry> path, [CanBeNull] string fileName)
        {
            var fullName = $"{path.GetFullPath()}{fileName}";
            return fullName;
        }

        /// <summary>
        /// Appends parts to a path.
        /// </summary>
        /// <param name="path">The path to append to.</param>
        /// <param name="parts">The parts to append.</param>
        /// <returns>The combined path.</returns>
        [NotNull]
        public static string CombinePath([CanBeNull] string path, [NotNull, ItemNotNull] params string[] parts)
        {
            return CombinePath(path, (IEnumerable<string>)parts);
        }

        /// <summary>
        /// Appends parts to a path.
        /// </summary>
        /// <param name="path">The path to append to.</param>
        /// <param name="parts">The parts to append.</param>
        /// <returns>The combined path.</returns>
        [NotNull]
        public static string CombinePath([CanBeNull] string path, [NotNull, ItemNotNull] IEnumerable<string> parts)
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
                {
                    result.Append('/');
                }
                else
                {
                    addSlash = true;
                }

                result.Append(part.Replace("\\", "\\\\").Replace("/", "\\/"));
            }

            return result.ToString();
        }

        /// <summary>
        /// Split the path into parts.
        /// </summary>
        /// <param name="path">The path to split.</param>
        /// <returns>The parts of the path.</returns>
        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<string> SplitPath([CanBeNull] string path)
        {
            var parts = new List<string>();
            if (string.IsNullOrEmpty(path))
            {
                return parts;
            }

            var isEscaped = false;
            var partCollector = new StringBuilder();
            foreach (var ch in path)
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

        [NotNull]
        [ItemNotNull]
        private static IReadOnlyList<string> GetPathElements([CanBeNull] string path)
        {
            var pathElements = new List<string>();
            if (path != null && path.StartsWith("/"))
            {
                pathElements.Add(string.Empty);
            }

            var parts = SplitPath(path).Where(x => !string.IsNullOrEmpty(x));
            pathElements.AddRange(parts);
            return pathElements;
        }
    }
}
