// <copyright file="OneDriveFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acr.Cache;
using Acr.Cache.Impl;

using JetBrains.Annotations;
using RestSharp.Portable.Microsoft.OneDrive;
using RestSharp.Portable.Microsoft.OneDrive.Model;

namespace FubarDev.FtpServer.FileSystem.OneDrive
{
    /// <summary>
    /// The <see cref="IUnixFileSystem"/> implementation for OneDrive
    /// </summary>
    public class OneDriveFileSystem : IUnixFileSystem
    {
        private static readonly TimeSpan _defaultCacheTimeSpan = TimeSpan.FromSeconds(10);

        private readonly Dictionary<string, BackgroundUpload> _uploads = new Dictionary<string, BackgroundUpload>();

        private readonly SemaphoreSlim _uploadsLock = new SemaphoreSlim(1);

        private readonly OneDriveSupportFactory _supportFactory;

        private readonly ICache _cache = new InMemoryCacheImpl();

        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneDriveFileSystem"/> class.
        /// </summary>
        /// <param name="service">The OneDrive service instance to use for communication with the OneDrive API</param>
        /// <param name="supportFactory">A support factory instance to create some classes in a platform-agnostic way</param>
        /// <param name="drive">The drive to use</param>
        /// <param name="rootFolder">The root folder to use</param>
        public OneDriveFileSystem([NotNull] OneDriveService service, [NotNull] OneDriveSupportFactory supportFactory, [NotNull] Drive drive, [NotNull] Item rootFolder)
        {
            Service = service;
            _supportFactory = supportFactory;
            Drive = drive;
            RootFolderItem = rootFolder;
            Root = new OneDriveDirectoryEntry(this, rootFolder, true);
        }

        /// <summary>
        /// Gets the OneDrive API service
        /// </summary>
        [NotNull]
        public OneDriveService Service { get; }

        /// <summary>
        /// Gets the drive to use
        /// </summary>
        [NotNull]
        public Drive Drive { get; }

        /// <summary>
        /// Gets the root folder item
        /// </summary>
        [NotNull]
        public Item RootFolderItem { get; }

        /// <inheritdoc/>
        public bool SupportsAppend => false;

        /// <inheritdoc/>
        public bool SupportsNonEmptyDirectoryDelete => true;

        /// <inheritdoc/>
        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc/>
        public IUnixDirectoryEntry Root { get; }

        /// <summary>
        /// Creates a new instance of <see cref="OneDriveFileSystem"/>
        /// </summary>
        /// <param name="service">The OneDrive API service to use</param>
        /// <param name="supportFactory">The support factory to use</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The new <see cref="OneDriveFileSystem"/> instance</returns>
        /// <remarks>
        /// This function uses the users default drive and its root folder.
        /// </remarks>
        [NotNull, ItemNotNull]
        public static async Task<OneDriveFileSystem> Create([NotNull] OneDriveService service, [NotNull] OneDriveSupportFactory supportFactory, CancellationToken cancellationToken)
        {
            var drive = await service.GetDefaultDriveAsync(cancellationToken);
            return await Create(service, supportFactory, drive, cancellationToken);
        }

        /// <summary>
        /// Creates a new instance of <see cref="OneDriveFileSystem"/>
        /// </summary>
        /// <param name="service">The OneDrive API service to use</param>
        /// <param name="supportFactory">The support factory to use</param>
        /// <param name="drive">The drive to get the root folder from</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The new <see cref="OneDriveFileSystem"/> instance</returns>
        [NotNull, ItemNotNull]
        public static async Task<OneDriveFileSystem> Create([NotNull] OneDriveService service, [NotNull] OneDriveSupportFactory supportFactory, [NotNull] Drive drive, CancellationToken cancellationToken)
        {
            var rootFolderInfo = await service.GetRootFolderAsync(drive.Id, cancellationToken);
            return new OneDriveFileSystem(service, supportFactory, drive, rootFolderInfo);
        }

        /// <inheritdoc/>
        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            var targetFolderItem = ((OneDriveDirectoryEntry)targetDirectory).Item;
            var tempData = await _supportFactory.CreateTemporaryData(data, cancellationToken);
            var targetId = GetFileId(targetFolderItem.Id, fileName);
            var backgroundUploads = new BackgroundUpload(targetId, targetFolderItem.Id, fileName, tempData, this);
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                var fileId = GetFileId(targetFolderItem.Id, fileName);
                _uploads.Add(fileId, backgroundUploads);
            }
            finally
            {
                _uploadsLock.Release();
            }
            return backgroundUploads;
        }

        /// <inheritdoc/>
        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            var targetFolderItem = ((OneDriveDirectoryEntry)targetDirectory).Item;
            var folderItem = await Service.CreateFolderAsync(Drive.Id, targetFolderItem.Id, directoryName, cancellationToken);
            return new OneDriveDirectoryEntry(this, folderItem, false);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var item = ((OneDriveDirectoryEntry)directoryEntry).Item;
            var children = await Service.GetItemChildrenAsync(Drive.Id, item.Id, cancellationToken);
            return await ConvertToUnixFileSystemEntries(item.Id, children, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var item = ((OneDriveDirectoryEntry)directoryEntry).Item;
            var childId = GetFileId(item.Id, name);
            var child = await _cache.TryGet(childId, () => Service.GetChildItemAsync(Drive.Id, item.Id, name, cancellationToken), _defaultCacheTimeSpan);
            if (child == null)
            {
                await _uploadsLock.WaitAsync(cancellationToken);
                try
                {
                    var result = _uploads.Values
                                         .Where(x => x.ParentId == item.Id)
                                         .Select(x => new OneDriveFileEntry(this, x.Item, x.FileSize))
                                         .FirstOrDefault();
                    return result;
                }
                finally
                {
                    _uploadsLock.Release();
                }
            }
            return await ConvertToUnixFileSystemEntry(child, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            var sourceItem = ((OneDriveFileSystemEntry)source).Item;
            var sourceId = sourceItem.Id;
            var newParentItem = ((OneDriveDirectoryEntry)target).Item;
            var newParentRef = new ItemReference()
            {
                Id = newParentItem.Id,
            };
            _cache.Remove(GetFileId(sourceItem));
            var newItem = await Service.MoveAsync(Drive.Id, sourceId, fileName, newParentRef, cancellationToken);
            _cache.Set(GetFileId(newItem), newItem, _defaultCacheTimeSpan);
            return await ConvertToUnixFileSystemEntry(newItem, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var from = startPosition != 0 ? (long?)startPosition : null;
            var fileItem = ((OneDriveFileEntry)fileEntry).Item;
            var response = await Service.GetDownloadResponseAsync(Drive.Id, fileItem.Id, from, cancellationToken);
            return new OneDriveDownloadStream(this, fileItem, response, startPosition, fileEntry.Size);
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var fileItem = ((OneDriveFileEntry)fileEntry).Item;
            var parentId = fileItem.ParentReference.Id;
            var fileName = fileItem.Name;
            var tempData = await _supportFactory.CreateTemporaryData(data, cancellationToken);
            var targetId = GetFileId(parentId, fileName);
            var backgroundUploads = new BackgroundUpload(targetId, parentId, fileName, tempData, this);
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                var fileId = GetFileId(parentId, fileName);
                _uploads.Add(fileId, backgroundUploads);
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
            var item = ((OneDriveFileSystemEntry)entry).Item;

            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                var id = GetFileId(item);
                BackgroundUpload uploader;
                if (_uploads.TryGetValue(id, out uploader))
                {
                    if (uploader.ItemChanges == null)
                        uploader.ItemChanges = new Item();
                    if (uploader.ItemChanges.FileSystemInfo == null)
                        uploader.ItemChanges.FileSystemInfo = new FileSystemInfo();
                    if (item.FileSystemInfo == null)
                        item.FileSystemInfo = new FileSystemInfo();
                    if (uploader.Item.FileSystemInfo == null)
                        uploader.Item.FileSystemInfo = new FileSystemInfo();
                    if (modify != null)
                    {
                        uploader.ItemChanges.FileSystemInfo.LastModifiedDateTime = modify;
                        uploader.Item.FileSystemInfo.LastModifiedDateTime = modify;
                        item.FileSystemInfo.LastModifiedDateTime = modify;
                    }
                    if (create != null)
                    {
                        uploader.ItemChanges.FileSystemInfo.CreatedDateTime = create;
                        uploader.Item.FileSystemInfo.CreatedDateTime = create;
                        item.FileSystemInfo.CreatedDateTime = create;
                    }
                    return entry;
                }
            }
            finally
            {
                _uploadsLock.Release();
            }

            var updateItem = new Item() { FileSystemInfo = new FileSystemInfo() };
            if (modify != null)
                updateItem.FileSystemInfo.LastModifiedDateTime = modify;
            if (create != null)
                updateItem.FileSystemInfo.CreatedDateTime = create;

            var newItem = await Service.UpdateAsync(Drive.Id, item.Id, updateItem, cancellationToken);
            _cache.Set(GetFileId(newItem), newItem, _defaultCacheTimeSpan);
            if (newItem.Folder != null)
                return new OneDriveDirectoryEntry(this, newItem, false);
            return new OneDriveFileEntry(this, newItem, null);
        }

        /// <inheritdoc/>
        public async Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var entryItem = ((OneDriveFileSystemEntry)entry).Item;
            await Service.DeleteAsync(Drive.Id, entryItem.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        internal void DownloadFinished(Item item)
        {
            _cache.Set(GetFileId(item), item, _defaultCacheTimeSpan);
        }

        internal async Task UploadFinished(string parentId, string name, bool withError)
        {
            BackgroundUpload uploader;
            var id = GetFileId(parentId, name);

            _uploadsLock.Wait();
            try
            {
                uploader = _uploads[id];
                if (withError)
                {
                    _cache.Remove(id);
                }
                else
                {
                    _cache.Set(id, uploader.Item, _defaultCacheTimeSpan);
                }
                _uploads.Remove(id);
            }
            finally
            {
                _uploadsLock.Release();
            }

            if (!withError && uploader.ItemChanges != null)
            {
                var updatedItem = await Service.UpdateAsync(Drive.Id, uploader.Item.Id, uploader.ItemChanges, CancellationToken.None);
                _cache.Set(id, updatedItem, _defaultCacheTimeSpan);
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

        private static string GetFileId(Item item)
        {
            return GetFileId(item.ParentReference.Id, item.Name);
        }

        private static string GetFileId(string parentId, string name)
        {
            return $"items/{parentId}:/{name}";
        }

        [NotNull]
        [ItemNotNull]
        private async Task<IUnixFileSystemEntry> ConvertToUnixFileSystemEntry(Item item, CancellationToken cancellationToken)
        {
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                return ConvertToUnixFileSystemEntryLocked(item);
            }
            finally
            {
                _uploadsLock.Release();
            }
        }

        private IUnixFileSystemEntry ConvertToUnixFileSystemEntryLocked(Item item)
        {
            if (item.Folder != null)
            {
                return new OneDriveDirectoryEntry(this, item, false);
            }

            var id = GetFileId(item);
            long? fileSize;
            BackgroundUpload uploader;
            if (_uploads.TryGetValue(id, out uploader))
            {
                fileSize = uploader.FileSize;
            }
            else
            {
                fileSize = null;
            }

            return new OneDriveFileEntry(this, item, fileSize);
        }

        private async Task<IReadOnlyList<IUnixFileSystemEntry>> ConvertToUnixFileSystemEntries(string parentId, IEnumerable<Item> items, CancellationToken cancellationToken)
        {
            var result = new List<IUnixFileSystemEntry>();
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                var resultItems = items
                    .Where(x => x.Deleted == null)
                    .Select(ConvertToUnixFileSystemEntryLocked)
                    .ToList();
                var foundNames = new HashSet<string>(resultItems.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
                result.AddRange(resultItems);
                result.AddRange(
                    _uploads.Values
                            .Where(x => x.ParentId == parentId && !foundNames.Contains(x.Name))
                            .Select(x => new OneDriveFileEntry(this, x.Item, x.FileSize)));
            }
            finally
            {
                _uploadsLock.Release();
            }
            return result;
        }
    }
}
