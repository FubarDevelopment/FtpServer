// <copyright file="S3FileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using FubarDev.FtpServer.BackgroundTransfer;

namespace FubarDev.FtpServer.FileSystem.AzureBlobStorage
{
    /// <summary>
    /// The The Azure Blob Storage file system implementation.
    /// </summary>
    public sealed class AzureBlobStorageFileSystem : IUnixFileSystem
    {
        private readonly AzureBlobStorageFileSystemOptions _options;
        private readonly BlobServiceClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageFileSystem"/> class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        /// <param name="rootDirectory">The root container.</param>
        public AzureBlobStorageFileSystem(AzureBlobStorageFileSystemOptions options, string rootDirectory)
        {
            _options = options;
            _client = new BlobServiceClient(options.ConnectionString);
            Root = new AzureBlobStorageDirectoryEntry(rootDirectory, true);
        }

        /// <inheritdoc />
        public bool SupportsAppend => false;

        /// <inheritdoc />
        public bool SupportsNonEmptyDirectoryDelete => true;

        /// <inheritdoc />
        public StringComparer FileSystemEntryComparer => StringComparer.Ordinal;

        /// <inheritdoc />
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc />
        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(
            IUnixDirectoryEntry directoryEntry,
            CancellationToken cancellationToken)
        {
            var prefix = ((AzureBlobStorageDirectoryEntry)directoryEntry).Key;
            prefix = prefix.TrimStart('/');
            return ListObjectsAsync(prefix, false, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IUnixFileSystemEntry?> GetEntryByNameAsync(
            IUnixDirectoryEntry directoryEntry,
            string name,
            CancellationToken cancellationToken)
        {
            var key = AzureBlobStoragePath.Combine(((AzureBlobStorageDirectoryEntry)directoryEntry).Key, name);

            var entry = await GetObjectAsync(key, cancellationToken);
            if (entry != null)
                return entry;

            // not a file search for directory
            key += '/';

            var objects = await ListObjectsAsync(key, true, cancellationToken);
            if (objects.Count > 0)
                return new AzureBlobStorageDirectoryEntry(key);

            return null;
        }

        /// <inheritdoc />
        public async Task<IUnixFileSystemEntry> MoveAsync(
            IUnixDirectoryEntry parent,
            IUnixFileSystemEntry source,
            IUnixDirectoryEntry target,
            string fileName,
            CancellationToken cancellationToken)
        {
            var sourceKey = ((AzureBlobStorageDirectoryEntry)source).Key;
            var key = AzureBlobStoragePath.Combine(((AzureBlobStorageDirectoryEntry)target).Key, fileName);

            if (source is AzureBlobStorageFileEntry file)
            {
                await MoveFile(sourceKey, key, cancellationToken);
                return new AzureBlobStorageFileEntry(key, file.Size)
                {
                    LastWriteTime = file.LastWriteTime ?? DateTimeOffset.UtcNow,
                };
            }

            if (source is AzureBlobStorageDirectoryEntry)
            {
                key += '/';
                var container = _client.GetBlobContainerClient(_options.ContainerName);
                var response = container.GetBlobsAsync(prefix: key);

                var enumerator = response.AsPages().GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync())
                {
                    var blob = enumerator.Current;
                    if (blob == null) continue;
                    foreach (var item in blob.Values)
                    {
                        await MoveFile(item.Name, key + item.Name.Substring(sourceKey.Length), cancellationToken);
                    }
                }

                return new AzureBlobStorageDirectoryEntry(key);
            }

            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var blobClient = GetBlobClient(((AzureBlobStorageFileSystemEntry)entry).Key);
            return blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(
            IUnixDirectoryEntry targetDirectory,
            string directoryName,
            CancellationToken cancellationToken)
        {
            var key = AzureBlobStoragePath.Combine(((AzureBlobStorageDirectoryEntry)targetDirectory).Key, directoryName + "/");

            await using var memoryStream = new MemoryStream();

            memoryStream.Write(Encoding.UTF8.GetBytes(""));
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Directories are virtual in azure blob storage if empty, upload an empty file as a workaround
            await UploadFile(memoryStream, key + ".azuredir", cancellationToken);

            return new AzureBlobStorageDirectoryEntry(key);
        }

        /// <inheritdoc />
        public async Task<Stream> OpenReadAsync(
            IUnixFileEntry fileEntry,
            long startPosition,
            CancellationToken cancellationToken)
        {
            var blobClient = GetBlobClient(((AzureBlobStorageFileSystemEntry)fileEntry).Key);

            if (!await blobClient.ExistsAsync()) return Stream.Null;

            var stream = await blobClient.OpenReadAsync(startPosition);

            if (startPosition != 0)
            {
                stream.Seek(startPosition, SeekOrigin.Begin);
            }

            return stream;
        }

        /// <inheritdoc />
        public Task<IBackgroundTransfer?> AppendAsync(
            IUnixFileEntry fileEntry,
            long? startPosition,
            Stream data,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer?> CreateAsync(
            IUnixDirectoryEntry targetDirectory,
            string fileName,
            Stream data,
            CancellationToken cancellationToken)
        {
            var key = AzureBlobStoragePath.Combine(((AzureBlobStorageDirectoryEntry)targetDirectory).Key, fileName);
            await UploadFile(data, key, cancellationToken);
            return default;
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer?> ReplaceAsync(
            IUnixFileEntry fileEntry,
            Stream data,
            CancellationToken cancellationToken)
        {
            await UploadFile(data, ((AzureBlobStorageFileEntry)fileEntry).Key, cancellationToken);
            return default;
        }

        /// <inheritdoc />
        public Task<IUnixFileSystemEntry> SetMacTimeAsync(
            IUnixFileSystemEntry entry,
            DateTimeOffset? modify,
            DateTimeOffset? access,
            DateTimeOffset? create,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(entry);
        }

        private async Task<IUnixFileSystemEntry?> GetObjectAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                var container = _client.GetBlobContainerClient(_options.ContainerName);

                var response = container.GetBlobsAsync(prefix: key);
                var enumerator = response.AsPages().GetAsyncEnumerator();
                await enumerator.MoveNextAsync();

                if (enumerator.Current.Values.Count < 1) return null;

                if (key.EndsWith("/"))
                    return new AzureBlobStorageDirectoryEntry(key);

                var item = enumerator.Current.Values.Where(x => x.Name == key).FirstOrDefault();

                if (item == null) return null;

                return new AzureBlobStorageFileEntry(key, item.Properties.ContentLength.GetValueOrDefault())
                {
                    LastWriteTime = item.Properties.LastModified,
                };
            }
            catch (Exception) { }

            return null;
        }

        private async Task<IReadOnlyList<IUnixFileSystemEntry>> ListObjectsAsync(
            string prefix,
            bool includeSelf,
            CancellationToken cancellationToken)
        {
            var objects = new List<IUnixFileSystemEntry>();

            var container = _client.GetBlobContainerClient(_options.ContainerName);

            var response = container.GetBlobsByHierarchyAsync(delimiter: "/", prefix: prefix);
            var enumerator = response.AsPages().GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync())
            {
                var blob = enumerator.Current;
                if (blob == null) continue;
                foreach (var item in blob.Values)
                {
                    if (item.IsPrefix)
                    {
                        objects.Add(new AzureBlobStorageDirectoryEntry(item.Prefix));
                    }
                    else if (item.IsBlob)
                    {
                        objects.Add(new AzureBlobStorageFileEntry(item.Blob.Name, item.Blob.Properties.ContentLength.GetValueOrDefault()));
                    }
                }
            }

            return objects;
        }

        private async Task MoveFile(string sourceKey, string key, CancellationToken cancellationToken)
        {
            var blobClient = GetBlobClient(sourceKey);

            var tempDownload = await blobClient.DownloadStreamingAsync();
            await UploadFile(tempDownload.Value.Content, key, cancellationToken);

            await blobClient.DeleteAsync();
        }

        private async Task UploadFile(Stream data, string key, CancellationToken cancellationToken)
        {
            var blobClient = GetBlobClient(key);
            try
            {
                var response = await blobClient.UploadAsync(data, true, cancellationToken);
            }
            catch (Exception) { }
        }

        private BlobClient GetBlobClient(string key)
        {
            var container = _client.GetBlobContainerClient(_options.ContainerName);
            return container.GetBlobClient(key);
        }
    }
}
