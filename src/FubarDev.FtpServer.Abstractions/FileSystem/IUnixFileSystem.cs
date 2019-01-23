//-----------------------------------------------------------------------
// <copyright file="IUnixFileSystem.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// The unix file system abstraction used by the FTP server.
    /// </summary>
    public interface IUnixFileSystem
    {
        /// <summary>
        /// Gets a value indicating whether this file system supports appending to a file.
        /// </summary>
        bool SupportsAppend { get; }

        /// <summary>
        /// Gets a value indicating whether this file system supports deletion of non-empty directories.
        /// </summary>
        bool SupportsNonEmptyDirectoryDelete { get; }

        /// <summary>
        /// Gets a string comparer for file system entry names.
        /// </summary>
        /// <remarks>
        /// Required to support case insensitive file systems.
        /// </remarks>
        [NotNull]
        StringComparer FileSystemEntryComparer { get; }

        /// <summary>
        /// Gets the root directory entry.
        /// </summary>
        [NotNull]
        IUnixDirectoryEntry Root { get; }

        /// <summary>
        /// Gets a list of <see cref="IUnixFileSystemEntry"/> objects for a given <paramref name="directoryEntry"/>.
        /// </summary>
        /// <param name="directoryEntry">The <see cref="IUnixDirectoryEntry"/> to get the file system entries for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of <see cref="IUnixFileSystemEntry"/> objects for a given <paramref name="directoryEntry"/>.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync([NotNull] IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a file system entry by name.
        /// </summary>
        /// <param name="directoryEntry">The <see cref="IUnixDirectoryEntry"/> to get the file system entry for.</param>
        /// <param name="name">The name of the file system entry to search.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns the found <see cref="IUnixFileSystemEntry"/>.</returns>
        [NotNull]
        [ItemCanBeNull]
        Task<IUnixFileSystemEntry> GetEntryByNameAsync([NotNull] IUnixDirectoryEntry directoryEntry, [NotNull] string name, CancellationToken cancellationToken);

        /// <summary>
        /// Moves a file system entry from <paramref name="parent"/> to <paramref name="target"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IUnixDirectoryEntry"/> of <paramref name="source"/>.</param>
        /// <param name="source">The <see cref="IUnixFileSystemEntry"/> to move.</param>
        /// <param name="target">The destination <see cref="IUnixDirectoryEntry"/> where <paramref name="source"/> gets moved to.</param>
        /// <param name="fileName">The new name of <paramref name="source"/>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns the new <see cref="IUnixFileSystemEntry"/>.</returns>
        [NotNull]
        Task<IUnixFileSystemEntry> MoveAsync([NotNull] IUnixDirectoryEntry parent, [NotNull] IUnixFileSystemEntry source, [NotNull] IUnixDirectoryEntry target, [NotNull] string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a <see cref="IUnixFileSystemEntry"/>.
        /// </summary>
        /// <param name="entry">The <see cref="IUnixFileSystemEntry"/> to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task where the underlying action is performed on.</returns>
        /// <remarks>
        /// An implementation might decide to move the <paramref name="entry"/> into the trash instead of deleting it.
        /// </remarks>
        [NotNull]
        Task UnlinkAsync([NotNull] IUnixFileSystemEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a sub directory with the name <paramref name="directoryName"/> in <paramref name="targetDirectory"/>.
        /// </summary>
        /// <param name="targetDirectory">The directory to create the sub directory in.</param>
        /// <param name="directoryName">The name of the new sub directory.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The new <see cref="IUnixDirectoryEntry"/>.</returns>
        [NotNull]
        Task<IUnixDirectoryEntry> CreateDirectoryAsync([NotNull] IUnixDirectoryEntry targetDirectory, [NotNull] string directoryName, CancellationToken cancellationToken);

        /// <summary>
        /// Opens a file for reading.
        /// </summary>
        /// <param name="fileEntry">The <see cref="IUnixFileEntry"/> to read from.</param>
        /// <param name="startPosition">The start position to read from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Stream"/> to be used for reading.</returns>
        [NotNull]
        Task<Stream> OpenReadAsync([NotNull] IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken);

        /// <summary>
        /// Appends data to a file.
        /// </summary>
        /// <param name="fileEntry">The <see cref="IUnixFileEntry"/> to append data to.</param>
        /// <param name="startPosition">The start position - when it is null, the data is appended.</param>
        /// <param name="data">The data stream to read from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>an optional <see cref="IBackgroundTransfer"/> when the transfer needs to happen in the background.</returns>
        [NotNull]
        [ItemCanBeNull]
        Task<IBackgroundTransfer> AppendAsync([NotNull] IUnixFileEntry fileEntry, long? startPosition, [NotNull] Stream data, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a file with the given <paramref name="fileName"/> and <paramref name="data"/>.
        /// </summary>
        /// <param name="targetDirectory">The directory to create the file in.</param>
        /// <param name="fileName">The name of the new file.</param>
        /// <param name="data">The <see cref="Stream"/> used to read the data for the new file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>an optional <see cref="IBackgroundTransfer"/> when the transfer needs to happen in the background.</returns>
        [NotNull]
        [ItemCanBeNull]
        Task<IBackgroundTransfer> CreateAsync([NotNull] IUnixDirectoryEntry targetDirectory, [NotNull] string fileName, [NotNull] Stream data, CancellationToken cancellationToken);

        /// <summary>
        /// Replaces the contents of a file.
        /// </summary>
        /// <param name="fileEntry">The <see cref="IUnixFileEntry"/> to replace the data for.</param>
        /// <param name="data">The data to be written to the given <paramref name="fileEntry"/>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>an optional <see cref="IBackgroundTransfer"/> when the transfer needs to happen in the background.</returns>
        [NotNull]
        [ItemCanBeNull]
        Task<IBackgroundTransfer> ReplaceAsync([NotNull] IUnixFileEntry fileEntry, [NotNull] Stream data, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the modify/access/create timestamp of a file system item.
        /// </summary>
        /// <param name="entry">The <see cref="IUnixFileSystemEntry"/> to change the timestamp for.</param>
        /// <param name="modify">The modification timestamp.</param>
        /// <param name="access">The access timestamp.</param>
        /// <param name="create">The creation timestamp.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The modified <see cref="IUnixFileSystemEntry"/>.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IUnixFileSystemEntry> SetMacTimeAsync([NotNull] IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken);
    }
}
