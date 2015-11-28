//-----------------------------------------------------------------------
// <copyright file="GoogleDriveFileSystem.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using RestSharp.Portable;
using RestSharp.Portable.Google.Drive;

using File = RestSharp.Portable.Google.Drive.Model.File;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// The <see cref="IUnixFileSystem"/> implementation that uses Google Drive
    /// </summary>
    public class GoogleDriveFileSystem : IUnixFileSystem
    {
        private readonly Dictionary<string, BackgroundUpload> _uploads = new Dictionary<string, BackgroundUpload>();

        private readonly SemaphoreSlim _uploadsLock = new SemaphoreSlim(1);

        private readonly GoogleDriveSupportFactory _requestFactory;

        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveFileSystem"/> class.
        /// </summary>
        /// <param name="service">The <see cref="GoogleDriveService"/> instance to use to access the Google Drive</param>
        /// <param name="rootFolderInfo">The <see cref="File"/> to use as root folder</param>
        /// <param name="requestFactory">A <see cref="IRequestFactory"/> used to create <see cref="IRestClient"/> and <see cref="HttpWebRequest"/> objects</param>
        public GoogleDriveFileSystem(GoogleDriveService service, File rootFolderInfo, GoogleDriveSupportFactory requestFactory)
        {
            _requestFactory = requestFactory;
            Service = service;
            RootFolderInfo = rootFolderInfo;
            Root = new GoogleDriveDirectoryEntry(this, RootFolderInfo, "/", true);
        }

        /// <summary>
        /// Gets the <see cref="GoogleDriveService"/> instance to use to access the Google Drive
        /// </summary>
        public GoogleDriveService Service { get; }

        /// <summary>
        /// Gets the <see cref="File"/> to use as root folder
        /// </summary>
        public File RootFolderInfo { get; }

        /// <inheritdoc/>
        public bool SupportsNonEmptyDirectoryDelete => true;

        /// <inheritdoc/>
        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc/>
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc/>
        public bool SupportsAppend => false;

        /// <inheritdoc/>
        public async Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)directoryEntry;
            var entries = await ConvertEntries(
                dirEntry,
                () => Service.GetChildrenAsync(dirEntry.File, cancellationToken),
                cancellationToken);
            return entries;
        }

        /// <inheritdoc/>
        public async Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)directoryEntry;
            var entries = await ConvertEntries(
                dirEntry,
                () => Service.FindChildByNameAsync(dirEntry.File, name, cancellationToken),
                cancellationToken);
            return entries.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            var parentEntry = (GoogleDriveDirectoryEntry)parent;
            var targetEntry = (GoogleDriveDirectoryEntry)target;
            var targetName = FileSystemExtensions.CombinePath(targetEntry.FullName, fileName);

            var sourceFileEntry = source as GoogleDriveFileEntry;
            if (sourceFileEntry != null)
            {
                var newFile = await Service.MoveAsync(sourceFileEntry.File, parentEntry.File.Id, targetEntry.File, fileName, cancellationToken);
                return new GoogleDriveFileEntry(this, newFile, targetName);
            }

            var sourceDirEntry = (GoogleDriveDirectoryEntry)source;
            var newDir = await Service.MoveAsync(sourceDirEntry.File, parentEntry.File.Id, targetEntry.File, fileName, cancellationToken);
            return new GoogleDriveDirectoryEntry(this, newDir, targetName);
        }

        /// <inheritdoc/>
        public async Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var dirEntry = entry as GoogleDriveDirectoryEntry;
            if (dirEntry != null)
            {
                await Service.TrashAsync(dirEntry.File, cancellationToken);
            }
            else
            {
                var fileEntry = (GoogleDriveFileEntry)entry;
                await Service.TrashAsync(fileEntry.File, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)targetDirectory;
            var newDir = await Service.CreateDirectoryAsync(dirEntry.File, directoryName, cancellationToken);
            return new GoogleDriveDirectoryEntry(this, newDir, FileSystemExtensions.CombinePath(dirEntry.FullName, newDir.Title));
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var from = startPosition != 0 ? (long?)startPosition : null;
            var fe = (GoogleDriveFileEntry)fileEntry;
            var response = await Service.GetDownloadResponseAsync(fe.File, from, cancellationToken);
            return new GoogleDriveDownloadStream(response, startPosition, fe.Size);
        }

        /// <inheritdoc/>
        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            var targetEntry = (GoogleDriveDirectoryEntry)targetDirectory;
            var newFileEntry = await Service.CreateItemAsync(targetEntry.File, fileName, cancellationToken);
            var tempData = await _requestFactory.CreateTemporaryData(data, cancellationToken);
            var fullPath = FileSystemExtensions.CombinePath(targetEntry.FullName, fileName);
            var backgroundUploads = new BackgroundUpload(fullPath, newFileEntry, tempData, this);
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                _uploads.Add(backgroundUploads.File.Id, backgroundUploads);
            }
            finally
            {
                _uploadsLock.Release();
            }
            return backgroundUploads;
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var fe = (GoogleDriveFileEntry)fileEntry;
            var tempData = await _requestFactory.CreateTemporaryData(data, cancellationToken);
            var backgroundUploads = new BackgroundUpload(fe.FullName, fe.File, tempData, this);
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                _uploads.Add(backgroundUploads.File.Id, backgroundUploads);
            }
            finally
            {
                _uploadsLock.Release();
            }
            return backgroundUploads;
        }

        /// <inheritdoc/>
        public async Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            var dirEntry = entry as GoogleDriveDirectoryEntry;
            var fileEntry = entry as GoogleDriveFileEntry;
            var item = dirEntry == null ? fileEntry?.File : dirEntry.File;
            if (item == null)
                throw new InvalidOperationException();
            var newItemValues = new File()
            {
                ModifiedDate = modify?.UtcDateTime,
                CreatedDate = create?.UtcDateTime,
                LastViewedByMeDate = access?.UtcDateTime,
            };
            var newItem = await Service.UpdateAsync(item.Id, newItemValues, cancellationToken);
            var fullName = dirEntry == null ? fileEntry.FullName : dirEntry.FullName;
            var targetFullName = FileSystemExtensions.CombinePath(fullName.GetParentPath(), newItem.Title);
            if (dirEntry != null)
                return new GoogleDriveDirectoryEntry(this, newItem, targetFullName, dirEntry.IsRoot);
            return new GoogleDriveFileEntry(this, newItem, fullName, fileEntry.Size);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Is called when the upload is finished.
        /// </summary>
        /// <param name="fileId">The ID of the file to be notified as finished.</param>
        internal void UploadFinished(string fileId)
        {
            _uploadsLock.Wait();
            try
            {
                _uploads.Remove(fileId);
            }
            finally
            {
                _uploadsLock.Release();
            }
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <param name="disposing"><code>true</code> when called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Service.Dispose();
                    _uploadsLock.Dispose();
                }

                _disposedValue = true;
            }
        }

        private async Task<IReadOnlyList<IUnixFileSystemEntry>> ConvertEntries(GoogleDriveDirectoryEntry dirEntry, Func<Task<IReadOnlyList<File>>> getEntriesFunc, CancellationToken cancellationToken)
        {
            var result = new List<IUnixFileSystemEntry>();
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                var baseDir = dirEntry.FullName;
                foreach (var child in (await getEntriesFunc()).Where(x => x.Labels == null || !x.Labels.Trashed))
                {
                    var fullName = FileSystemExtensions.CombinePath(baseDir, child.Title);
                    if (child.IsDirectory())
                    {
                        result.Add(new GoogleDriveDirectoryEntry(this, child, fullName));
                    }
                    else
                    {
                        long? fileSize;
                        BackgroundUpload uploader;
                        if (_uploads.TryGetValue(child.Id, out uploader))
                        {
                            fileSize = uploader.FileSize;
                        }
                        else
                        {
                            fileSize = null;
                        }
                        result.Add(new GoogleDriveFileEntry(this, child, fullName, fileSize));
                    }
                }
            }
            finally
            {
                _uploadsLock.Release();
            }
            return result;
        }
    }
}
