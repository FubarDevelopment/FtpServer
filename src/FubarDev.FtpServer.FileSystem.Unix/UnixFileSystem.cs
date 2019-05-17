// <copyright file="UnixFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.BackgroundTransfer;
using JetBrains.Annotations;
using Mono.Unix;
using Mono.Unix.Native;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    internal class UnixFileSystem : IUnixFileSystem
    {
        [NotNull]
        private readonly IFtpUser _user;

        [CanBeNull]
        private readonly UnixUserInfo _userInfo;

        [NotNull]
        private readonly UnixFileSystemOptions _options;

        public UnixFileSystem(
            [NotNull] IUnixDirectoryEntry root,
            [NotNull] IFtpUser user,
            [CanBeNull] UnixUserInfo userInfo,
            [NotNull] UnixFileSystemOptions options)
        {
            _user = user;
            _userInfo = userInfo;
            _options = options;
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
            using (ChangeIds())
            {
                var dirEntry = (UnixDirectoryEntry)directoryEntry;
                var dirInfo = dirEntry.Info;
                var entries = dirInfo.GetFileSystemEntries().Select(x => CreateEntry(dirEntry, x)).ToList();
                return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(entries);
            }
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            using (ChangeIds())
            {
                var dirEntry = (UnixDirectoryEntry)directoryEntry;
                var dirInfo = dirEntry.Info;
                var entry = dirInfo.GetFileSystemEntries($"^{Regex.Escape(name)}$")
                   .Select(x => CreateEntry(dirEntry, x))
                   .SingleOrDefault();
                return Task.FromResult(entry);
            }
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> MoveAsync(
            IUnixDirectoryEntry parent,
            IUnixFileSystemEntry source,
            IUnixDirectoryEntry target,
            string fileName,
            CancellationToken cancellationToken)
        {
            using (ChangeIds())
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
        }

        /// <inheritdoc />
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            using (ChangeIds())
            {
                var entryInfo = ((UnixFileSystemEntry)entry).GenericInfo;
                entryInfo.Delete();
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc />
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(
            IUnixDirectoryEntry targetDirectory,
            string directoryName,
            CancellationToken cancellationToken)
        {
            using (ChangeIds())
            {
                var targetEntry = (UnixDirectoryEntry)targetDirectory;
                var newDirectoryName = UnixPath.Combine(targetEntry.Info.FullName, directoryName);
                var newDirectoryInfo = new UnixDirectoryInfo(newDirectoryName);
                newDirectoryInfo.Create();
                return Task.FromResult((IUnixDirectoryEntry)CreateEntry(targetEntry, newDirectoryInfo));
            }
        }

        /// <inheritdoc />
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            using (ChangeIds())
            {
                var fileInfo = ((UnixFileEntry)fileEntry).Info;
                var stream = fileInfo.OpenRead();
                if (startPosition != 0)
                {
                    stream.Seek(startPosition, SeekOrigin.Begin);
                }

                return Task.FromResult<Stream>(stream);
            }
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            using (ChangeIds())
            {
                var fileInfo = ((UnixFileEntry)fileEntry).Info;
                var stream = fileInfo.Open(FileMode.Append);
                if (startPosition != null)
                {
                    stream.Seek(startPosition.Value, SeekOrigin.Begin);
                }

                await data.CopyToAsync(stream, 81920, cancellationToken)
                   .ConfigureAwait(false);

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer> CreateAsync(
            IUnixDirectoryEntry targetDirectory,
            string fileName,
            Stream data,
            CancellationToken cancellationToken)
        {
            using (ChangeIds())
            {
                var targetInfo = ((UnixDirectoryEntry)targetDirectory).Info;
                var fileInfo = new UnixFileInfo(UnixPath.Combine(targetInfo.FullName, fileName));
                var stream = fileInfo.Open(FileMode.CreateNew, FileAccess.Write, FilePermissions.DEFFILEMODE);

                await data.CopyToAsync(stream, 81920, cancellationToken)
                   .ConfigureAwait(false);

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            using (ChangeIds())
            {
                var fileInfo = ((UnixFileEntry)fileEntry).Info;
                var stream = fileInfo.Open(FileMode.Create, FileAccess.Write, FilePermissions.DEFFILEMODE);

                await data.CopyToAsync(stream, 81920, cancellationToken)
                   .ConfigureAwait(false);

                return null;
            }
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

            using (ChangeIds())
            {
                Syscall.utimes(entryInfo.FullName, times);

                entryInfo.Refresh();
                return Task.FromResult(entry);
            }
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

        [NotNull]
        private IUnixFileSystemEntry CreateEntry([NotNull] IUnixDirectoryEntry parent, [NotNull] UnixFileSystemInfo info)
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

        private IDisposable ChangeIds()
        {
            if (!_options.EnableUserIdSwitch)
            {
                return EmptyDisposable.Empty;
            }

            return new UnixFileSystemIdChanger(_userInfo);
        }

        private class EmptyDisposable : IDisposable
        {
            public static IDisposable Empty { get; } = new EmptyDisposable();

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }
    }
}
