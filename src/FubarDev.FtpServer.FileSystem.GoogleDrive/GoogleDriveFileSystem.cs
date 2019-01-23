// <copyright file="GoogleDriveFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;

using Google.Apis.Drive.v3;
using Google.Apis.Upload;

using JetBrains.Annotations;

using File = Google.Apis.Drive.v3.Data.File;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// The <see cref="IUnixFileSystem"/> implementation that uses Google Drive.
    /// </summary>
    public sealed class GoogleDriveFileSystem : IGoogleDriveFileSystem, IDisposable
    {
        [NotNull]
        private readonly ITemporaryDataFactory _temporaryDataFactory;

        private readonly bool _useBackgroundUpload;

        private readonly Dictionary<string, BackgroundUpload> _uploads = new Dictionary<string, BackgroundUpload>();

        private readonly SemaphoreSlim _uploadsLock = new SemaphoreSlim(1);

        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveFileSystem"/> class.
        /// </summary>
        /// <param name="service">The <see cref="DriveService"/> instance to use to access the Google Drive.</param>
        /// <param name="rootFolderInfo">The <see cref="Google.Apis.Drive.v3.Data.File"/> to use as root folder.</param>
        /// <param name="temporaryDataFactory">The factory to create temporary data objects.</param>
        /// <param name="useBackgroundUpload">Use the Google Drive uploader instead of the background uploader.</param>
        public GoogleDriveFileSystem(
            [NotNull] DriveService service,
            [NotNull] File rootFolderInfo,
            [NotNull] ITemporaryDataFactory temporaryDataFactory,
            bool useBackgroundUpload)
        {
            _temporaryDataFactory = temporaryDataFactory;
            _useBackgroundUpload = useBackgroundUpload;
            Service = service;
            Root = new GoogleDriveDirectoryEntry(this, rootFolderInfo, "/", true);
        }

        /// <inheritdoc/>
        public DriveService Service { get; }

        /// <inheritdoc/>
        public bool SupportsNonEmptyDirectoryDelete => true;

        /// <inheritdoc/>
        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc/>
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc/>
        public bool SupportsAppend => false;

        /// <inheritdoc/>
        public async Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(
            IUnixDirectoryEntry directoryEntry,
            CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)directoryEntry;
            var entries = await ConvertEntries(
                dirEntry,
                () => GetChildrenAsync(dirEntry.File, cancellationToken),
                cancellationToken);
            return entries;
        }

        /// <inheritdoc/>
        public async Task<IUnixFileSystemEntry> GetEntryByNameAsync(
            IUnixDirectoryEntry directoryEntry,
            string name,
            CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)directoryEntry;
            var entries = await ConvertEntries(
                dirEntry,
                () => FindChildByNameAsync(dirEntry.File, name, cancellationToken),
                cancellationToken);
            return entries.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IUnixFileSystemEntry> MoveAsync(
            IUnixDirectoryEntry parent,
            IUnixFileSystemEntry source,
            IUnixDirectoryEntry target,
            string fileName,
            CancellationToken cancellationToken)
        {
            var parentEntry = (GoogleDriveDirectoryEntry)parent;
            var targetEntry = (GoogleDriveDirectoryEntry)target;
            var targetName = FileSystemExtensions.CombinePath(targetEntry.FullName, fileName);

            if (source is GoogleDriveFileEntry sourceFileEntry)
            {
                var newFile = await MoveItem(
                    parentEntry.File.Id,
                    targetEntry.File.Id,
                    sourceFileEntry.File.Id,
                    fileName,
                    cancellationToken);
                return new GoogleDriveFileEntry(this, newFile, targetName);
            }
            else
            {
                var sourceDirEntry = (GoogleDriveDirectoryEntry)source;
                var newDir = await MoveItem(
                    parentEntry.File.Id,
                    targetEntry.File.Id,
                    sourceDirEntry.File.Id,
                    fileName,
                    cancellationToken);
                return new GoogleDriveDirectoryEntry(this, newDir, targetName);
            }
        }

        /// <inheritdoc/>
        public async Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var body = new File
            {
                Trashed = true,
            };

            if (entry is GoogleDriveDirectoryEntry dirEntry)
            {
                await Service.Files.Update(body, dirEntry.File.Id).ExecuteAsync(cancellationToken);
            }
            else
            {
                var fileEntry = (GoogleDriveFileEntry)entry;
                await Service.Files.Update(body, fileEntry.File.Id).ExecuteAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(
            IUnixDirectoryEntry targetDirectory,
            string directoryName,
            CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)targetDirectory;
            var body = new File
            {
                Name = directoryName,
                Parents = new List<string>()
                {
                    dirEntry.File.Id,
                },
            }.AsDirectory();

            var request = Service.Files.Create(body);
            request.Fields = FileExtensions.DefaultFileFields;
            var newDir = await request.ExecuteAsync(cancellationToken);

            return new GoogleDriveDirectoryEntry(
                this,
                newDir,
                FileSystemExtensions.CombinePath(dirEntry.FullName, newDir.Name));
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenReadAsync(
            IUnixFileEntry fileEntry,
            long startPosition,
            CancellationToken cancellationToken)
        {
            var from = startPosition != 0 ? (long?)startPosition : null;
            var fe = (GoogleDriveFileEntry)fileEntry;
            var request = Service.Files.Get(fe.File.Id);

            using (var msg = request.CreateRequest())
            {
                if (from != null)
                {
                    msg.Headers.Range = new RangeHeaderValue(from, null);
                }

                // Add alt=media to the query parameters.
                var uri = new UriBuilder(msg.RequestUri);
                if (uri.Query.Length <= 1)
                {
                    uri.Query = "alt=media";
                }
                else
                {
                    // Remove the leading '?'. UriBuilder.Query doesn't round-trip.
                    uri.Query = uri.Query.Substring(1) + "&alt=media";
                }

                msg.RequestUri = uri.Uri;

                var response = await request.Service.HttpClient.SendAsync(
                        msg,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseStream = await response.Content.ReadAsStreamAsync();
                return new GoogleDriveDownloadStream(response, responseStream, startPosition, fe.Size);
            }
        }

        /// <inheritdoc/>
        public Task<IBackgroundTransfer> AppendAsync(
            IUnixFileEntry fileEntry,
            long? startPosition,
            Stream data,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Resuming uploads is not supported for non-seekable streams.");
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> CreateAsync(
            IUnixDirectoryEntry targetDirectory,
            string fileName,
            Stream data,
            CancellationToken cancellationToken)
        {
            var targetEntry = (GoogleDriveDirectoryEntry)targetDirectory;

            var body = new File
            {
                Name = fileName,
                Parents = new List<string>()
                {
                    targetEntry.File.Id,
                },
            };

            var request = Service.Files.Create(body);
            request.Fields = FileExtensions.DefaultFileFields;
            var newFileEntry = await request.ExecuteAsync(cancellationToken);

            if (!_useBackgroundUpload)
            {
                var upload = Service.Files.Update(new File(), newFileEntry.Id, data, "application/octet-stream");
                var result = await upload.UploadAsync(cancellationToken);
                if (result.Status == UploadStatus.Failed)
                {
                    throw new Exception(result.Exception.Message, result.Exception);
                }

                return null;
            }

            var expectedSize = data.CanSeek ? data.Length : (long?)null;
            var tempData = await _temporaryDataFactory.CreateAsync(data, expectedSize, cancellationToken);
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
        public async Task<IBackgroundTransfer> ReplaceAsync(
            IUnixFileEntry fileEntry,
            Stream data,
            CancellationToken cancellationToken)
        {
            var fe = (GoogleDriveFileEntry)fileEntry;

            if (!_useBackgroundUpload)
            {
                var upload = Service.Files.Update(new File(), fe.File.Id, data, "application/octet-stream");
                var result = await upload.UploadAsync(cancellationToken);
                if (result.Status == UploadStatus.Failed)
                {
                    throw new Exception(result.Exception.Message, result.Exception);
                }

                return null;
            }

            var expectedSize = data.CanSeek ? data.Length : (long?)null;
            var tempData = await _temporaryDataFactory.CreateAsync(data, expectedSize, cancellationToken);
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
        public async Task<IUnixFileSystemEntry> SetMacTimeAsync(
            IUnixFileSystemEntry entry,
            DateTimeOffset? modify,
            DateTimeOffset? access,
            DateTimeOffset? create,
            CancellationToken cancellationToken)
        {
            var dirEntry = entry as GoogleDriveDirectoryEntry;
            var fileEntry = entry as GoogleDriveFileEntry;
            var item = dirEntry == null ? fileEntry?.File : dirEntry.File;
            if (item == null)
            {
                throw new InvalidOperationException();
            }

            var newItemValues = new File()
            {
                ModifiedTime = modify?.UtcDateTime,
                CreatedTime = create?.UtcDateTime,
                ViewedByMeTime = access?.UtcDateTime,
            };

            var request = Service.Files.Update(newItemValues, item.Id);
            request.Fields = FileExtensions.DefaultFileFields;

            var newItem = await request.ExecuteAsync(cancellationToken);
            var fullName = dirEntry == null ? fileEntry.FullName : dirEntry.FullName;
            var targetFullName = FileSystemExtensions.CombinePath(fullName.GetParentPath(), newItem.Name);
            if (dirEntry != null)
            {
                return new GoogleDriveDirectoryEntry(this, newItem, targetFullName, dirEntry.IsRoot);
            }

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
        void IGoogleDriveFileSystem.UploadFinished(string fileId)
        {
            if (!_useBackgroundUpload)
            {
                // Nothing to do here...
                return;
            }

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
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing"><code>true</code> when called from <see cref="Dispose()"/>.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _uploadsLock.Dispose();
                }

                _disposedValue = true;
            }
        }

        private async Task<IReadOnlyList<IUnixFileSystemEntry>> ConvertEntries(
            GoogleDriveDirectoryEntry dirEntry,
            Func<Task<IReadOnlyCollection<File>>> getEntriesFunc,
            CancellationToken cancellationToken)
        {
            var result = new List<IUnixFileSystemEntry>();
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                var baseDir = dirEntry.FullName;
                foreach (var child in (await getEntriesFunc()).Where(x => !x.Trashed.GetValueOrDefault()))
                {
                    var fullName = FileSystemExtensions.CombinePath(baseDir, child.Name);
                    if (child.IsDirectory())
                    {
                        result.Add(new GoogleDriveDirectoryEntry(this, child, fullName));
                    }
                    else
                    {
                        long? fileSize;
                        if (_uploads.TryGetValue(child.Id, out var uploader))
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

        private async Task<IReadOnlyCollection<File>> GetChildrenAsync(File parent, CancellationToken cancellationToken)
        {
            var request = Service.Files.List();
            request.Q = $"{parent.Id.ToJsonString()} in parents";
            request.Fields = FileExtensions.DefaultListFields;
            var response = await request.ExecuteAsync(cancellationToken);
            return response.Files as IReadOnlyList<File> ?? response.Files.ToList();
        }

        private async Task<IReadOnlyCollection<File>> FindChildByNameAsync(
            File parent,
            string name,
            CancellationToken cancellationToken)
        {
            var request = Service.Files.List();
            request.Q = $"{parent.Id.ToJsonString()} in parents and name={name.ToJsonString()}";
            request.Fields = FileExtensions.DefaultListFields;
            var response = await request.ExecuteAsync(cancellationToken);
            return response.Files as IReadOnlyList<File> ?? response.Files.ToList();
        }

        private Task<File> MoveItem(
            string oldParentId,
            string newParentId,
            string fileId,
            string fileName,
            CancellationToken cancellationToken)
        {
            var body = new File
            {
                Name = fileName,
            };

            var request = Service.Files.Update(body, fileId);

            request.RemoveParents = oldParentId;
            request.AddParents = newParentId;
            request.Fields = FileExtensions.DefaultFileFields;

            return request.ExecuteAsync(cancellationToken);
        }
    }
}
