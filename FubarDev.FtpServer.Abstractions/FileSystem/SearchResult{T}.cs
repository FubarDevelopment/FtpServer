//-----------------------------------------------------------------------
// <copyright file="SearchResult{T}.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// The result of a file system search operation.
    /// </summary>
    /// <typeparam name="T">The type of the found file system entry.</typeparam>
    public class SearchResult<T>
        where T : IUnixFileSystemEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult{T}"/> class.
        /// </summary>
        /// <param name="directoryEntry">The directory entry of the found <paramref name="fileEntry"/>.</param>
        /// <param name="fileEntry">The found <see cref="IUnixFileSystemEntry"/>.</param>
        /// <param name="fileName">The name of the <see cref="IUnixFileSystemEntry"/> to be searched for within the <paramref name="directoryEntry"/>.</param>
        public SearchResult([NotNull] IUnixDirectoryEntry directoryEntry, [CanBeNull] T fileEntry, [CanBeNull] string fileName)
        {
            Directory = directoryEntry;
            Entry = fileEntry;
            FileName = fileName;
        }

        /// <summary>
        /// Gets the <see cref="IUnixDirectoryEntry"/> where the <see cref="FileName"/> was searched.
        /// </summary>
        [NotNull]
        public IUnixDirectoryEntry Directory { get; }

        /// <summary>
        /// Gets the found <see cref="IUnixFileSystemEntry"/>.
        /// </summary>
        /// <remarks>
        /// <code>null</code> when the target entry could not be found.
        /// </remarks>
        [CanBeNull]
        public T Entry { get; }

        /// <summary>
        /// Gets the name of the <see cref="Entry"/>.
        /// </summary>
        /// <remarks>
        /// <code>null</code> when the found entry is a ROOT entry.
        /// </remarks>
        [CanBeNull]
        public string FileName { get; }
    }
}
