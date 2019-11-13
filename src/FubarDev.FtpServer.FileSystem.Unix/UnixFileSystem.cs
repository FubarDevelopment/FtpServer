// <copyright file="UnixFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Compatibility;
using FubarDev.FtpServer.BackgroundTransfer;

using Microsoft.Extensions.Logging;

using Mono.Unix;
using Mono.Unix.Native;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    /// <summary>
    /// A backend that uses Posix(?) API calls to access the file system.
    /// </summary>
    public class UnixFileSystem : IUnixFileSystem
    {
        private readonly ClaimsPrincipal _user;
        private readonly UnixUserInfo? _userInfo;
        private readonly bool _flushStream;
        private readonly ILogger<UnixFileSystem>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystem"/> class.
        /// </summary>
        /// <param name="root">The root directory.</param>
        /// <param name="user">The current user.</param>
        /// <param name="userInfo">The user information.</param>
        [Obsolete("Use the overload with ClaimsPrincipal.")]
        public UnixFileSystem(
            IUnixDirectoryEntry root,
            IFtpUser user,
            UnixUserInfo? userInfo)
        {
            _user = user.CreateClaimsPrincipal();
            _userInfo = userInfo;
            Root = root;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystem"/> class.
        /// </summary>
        /// <param name="root">The root directory.</param>
        /// <param name="user">The current user.</param>
        /// <param name="userInfo">The user information.</param>
        public UnixFileSystem(
            IUnixDirectoryEntry root,
            ClaimsPrincipal user,
            UnixUserInfo? userInfo)
            : this(root, user, userInfo, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystem"/> class.
        /// </summary>
        /// <param name="root">The root directory.</param>
        /// <param name="user">The current user.</param>
        /// <param name="userInfo">The user information.</param>
        /// <param name="flushStream">Flush the stream after every write operation.</param>
        public UnixFileSystem(
            IUnixDirectoryEntry root,
            ClaimsPrincipal user,
            UnixUserInfo? userInfo,
            bool flushStream)
            : this(root, user, userInfo, flushStream, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileSystem"/> class.
        /// </summary>
        /// <param name="root">The root directory.</param>
        /// <param name="user">The current user.</param>
        /// <param name="userInfo">The user information.</param>
        /// <param name="flushStream">Flush the stream after every write operation.</param>
        /// <param name="logger">The logger for this file system implementation.</param>
        public UnixFileSystem(
            IUnixDirectoryEntry root,
            ClaimsPrincipal user,
            UnixUserInfo? userInfo,
            bool flushStream,
            ILogger<UnixFileSystem>? logger)
        {
            _user = user;
            _userInfo = userInfo;
            _flushStream = flushStream;
            _logger = logger;
            Root = root;
        }

        /// <inheritdoc />
        public bool SupportsAppend { get; } = true;

        /// <inheritdoc />
        public bool SupportsNonEmptyDirectoryDelete { get; } = false;

        /// <inheritdoc />
        public StringComparer FileSystemEntryComparer { get; } = StringComparer.Ordinal;

        /// <inheritdoc />
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc />
        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var dirEntry = (UnixDirectoryEntry)directoryEntry;
            var dirInfo = dirEntry.Info;
            var entries = dirInfo.GetFileSystemEntries().Select(x => CreateEntry(dirEntry, x)).ToList();
            return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(entries);
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry?> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var dirEntry = (UnixDirectoryEntry)directoryEntry;
            var dirInfo = dirEntry.Info;
            var entry = dirInfo.GetFileSystemEntries($"^{Regex.Escape(name)}$")
               .Select(x => CreateEntry(dirEntry, x))
               .Cast<IUnixFileSystemEntry?>()
               .SingleOrDefault();
            return Task.FromResult(entry);
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> MoveAsync(
            IUnixDirectoryEntry parent,
            IUnixFileSystemEntry source,
            IUnixDirectoryEntry target,
            string fileName,
            CancellationToken cancellationToken)
        {
            var sourceInfo = ((UnixFileSystemEntry)source).GenericInfo;
            var targetEntry = (UnixDirectoryEntry)target;
            var targetInfo = targetEntry.Info;
            var sourceEntryName = sourceInfo.FullName;
            var targetEntryName = UnixPath.Combine(targetInfo.FullName, fileName);
            if (Stdlib.rename(sourceEntryName, targetEntryName) == -1)
            {
                throw new InvalidOperationException("The entry couldn't be moved.");
            }

            var targetEntryInfo = UnixFileSystemInfo.GetFileSystemEntry(targetEntryName);
            return Task.FromResult(CreateEntry(targetEntry, targetEntryInfo));
        }

        /// <inheritdoc />
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var entryInfo = ((UnixFileSystemEntry)entry).GenericInfo;
            entryInfo.Delete();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(
            IUnixDirectoryEntry targetDirectory,
            string directoryName,
            CancellationToken cancellationToken)
        {
            var targetEntry = (UnixDirectoryEntry)targetDirectory;
            var newDirectoryName = UnixPath.Combine(targetEntry.Info.FullName, directoryName);
            var newDirectoryInfo = new UnixDirectoryInfo(newDirectoryName);
            newDirectoryInfo.Create();
            return Task.FromResult((IUnixDirectoryEntry)CreateEntry(targetEntry, newDirectoryInfo));
        }

        /// <inheritdoc />
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var fileInfo = ((UnixFileEntry)fileEntry).Info;
            var stream = fileInfo.OpenRead();
            if (startPosition != 0)
            {
                stream.Seek(startPosition, SeekOrigin.Begin);
            }

            return Task.FromResult<Stream>(stream);
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer?> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            var fileInfo = ((UnixFileEntry)fileEntry).Info;

            _logger?.LogTrace("Start appending to {fileName}", fileInfo.FullName);
            using (var stream = fileInfo.Open(FileMode.Append))
            {
                if (startPosition != null)
                {
                    stream.Seek(startPosition.Value, SeekOrigin.Begin);
                }

                /* Must be ConfigureAwait(true) to stay in the same synchronization context. */
                await data.CopyToAsync(stream, 81920, _flushStream, cancellationToken)
                   .ConfigureAwait(true);
                _logger?.LogTrace("Closing {fileName}", fileInfo.FullName);
            }

            _logger?.LogTrace("Closed {fileName}", fileInfo.FullName);

            return null;
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer?> CreateAsync(
            IUnixDirectoryEntry targetDirectory,
            string fileName,
            Stream data,
            CancellationToken cancellationToken)
        {
            var targetInfo = ((UnixDirectoryEntry)targetDirectory).Info;
            var fileInfo = new UnixFileInfo(UnixPath.Combine(targetInfo.FullName, fileName));

            _logger?.LogTrace("Start writing to {fileName}", fileInfo.FullName);
            using (var stream = fileInfo.Open(FileMode.CreateNew, FileAccess.Write, FilePermissions.DEFFILEMODE))
            {
                /* Must be ConfigureAwait(true) to stay in the same synchronization context. */
                await data.CopyToAsync(stream, 81920, _flushStream, cancellationToken)
                   .ConfigureAwait(true);
                _logger?.LogTrace("Closing {fileName}", fileInfo.FullName);
            }

            _logger?.LogTrace("Closed {fileName}", fileInfo.FullName);

            return null;
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer?> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var fileInfo = ((UnixFileEntry)fileEntry).Info;
            _logger?.LogTrace("Start writing to {fileName} while replacing old content", fileInfo.FullName);
            using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write, FilePermissions.DEFFILEMODE))
            {
                /* Must be ConfigureAwait(true) to stay in the same synchronization context. */
                await data.CopyToAsync(stream, 81920, _flushStream, cancellationToken)
                   .ConfigureAwait(true);
                _logger?.LogTrace("Closing {fileName}", fileInfo.FullName);
            }

            _logger?.LogTrace("Closed {fileName}", fileInfo.FullName);

            return null;
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> SetMacTimeAsync(
            IUnixFileSystemEntry entry,
            DateTimeOffset? modify,
            DateTimeOffset? access,
            DateTimeOffset? create,
            CancellationToken cancellationToken)
        {
            if (access == null && modify == null)
            {
                return Task.FromResult(entry);
            }

            var entryInfo = ((UnixFileSystemEntry)entry).GenericInfo;

            var times = new Timeval[2];

            if (access != null)
            {
                times[0] = ToTimeval(access.Value.UtcDateTime);
            }
            else
            {
                times[0] = ToTimeval(entryInfo.LastAccessTimeUtc);
            }

            if (modify != null)
            {
                times[1] = ToTimeval(modify.Value.UtcDateTime);
            }
            else
            {
                times[1] = ToTimeval(entryInfo.LastWriteTimeUtc);
            }

            Syscall.utimes(entryInfo.FullName, times);

            entryInfo.Refresh();
            return Task.FromResult(entry);
        }

        private static Timeval ToTimeval(DateTime timestamp)
        {
            var accessTicks = timestamp.ToUniversalTime().Ticks - NativeConvert.UnixEpoch.Ticks;
            var seconds = accessTicks / 10_000_000;
            var microseconds = (accessTicks % 10_000_000) / 10;
            return new Timeval()
            {
                tv_sec = seconds,
                tv_usec = microseconds,
            };
        }
        private IUnixFileSystemEntry CreateEntry(IUnixDirectoryEntry parent, UnixFileSystemInfo info)
        {
            switch (info)
            {
                case UnixFileInfo fileInfo:
                    return new UnixFileEntry(fileInfo);
                case UnixDirectoryInfo dirInfo:
                    return new UnixDirectoryEntry(dirInfo, _user, _userInfo, parent);
                default:
                    throw new NotSupportedException($"Unsupported file system info type {info}");
            }
        }
    }
}
