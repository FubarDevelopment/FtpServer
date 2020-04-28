// <copyright file="S3FileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

using FubarDev.FtpServer.BackgroundTransfer;

namespace FubarDev.FtpServer.FileSystem.S3
{
    /// <summary>
    /// The S3-based file system implementation.
    /// </summary>
    public sealed class S3FileSystem : IUnixFileSystem
    {
        private readonly S3FileSystemOptions _options;
        private readonly AmazonS3Client _client;
        private readonly TransferUtility _transferUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3FileSystem"/> class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        /// <param name="rootDirectory">The root directory for the current user.</param>
        public S3FileSystem(S3FileSystemOptions options, string rootDirectory)
        {
            _options = options;
            _client = new AmazonS3Client(
                options.AwsAccessKeyId,
                options.AwsSecretAccessKey,
                RegionEndpoint.GetBySystemName(options.BucketRegion));
            Root = new S3DirectoryEntry(rootDirectory, true);
            _transferUtility = new TransferUtility(_client);
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
            var prefix = ((S3DirectoryEntry)directoryEntry).Key;
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith("/"))
            {
                prefix += '/';
            }

            return ListObjectsAsync(prefix, false, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IUnixFileSystemEntry?> GetEntryByNameAsync(
            IUnixDirectoryEntry directoryEntry,
            string name,
            CancellationToken cancellationToken)
        {
            var key = S3Path.Combine(((S3DirectoryEntry)directoryEntry).Key, name);

            var entry = await GetObjectAsync(key, cancellationToken);
            if (entry != null)
                return entry;

            // not a file search for directory
            key += '/';

            var objects = await ListObjectsAsync(key, true, cancellationToken);
            if (objects.Count > 0)
                return new S3DirectoryEntry(key);

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
            var sourceKey = ((S3FileSystemEntry)source).Key;
            var key = S3Path.Combine(((S3DirectoryEntry)target).Key, fileName);

            if (source is S3FileEntry file)
            {
                await MoveFile(sourceKey, key, cancellationToken);
                return new S3FileEntry(key, file.Size)
                {
                    LastWriteTime = file.LastWriteTime ?? DateTimeOffset.UtcNow,
                };
            }

            if (source is S3DirectoryEntry)
            {
                key += '/';
                ListObjectsResponse response;
                do
                {
                    response = await _client.ListObjectsAsync(_options.BucketName, sourceKey, cancellationToken);
                    foreach (var s3Object in response.S3Objects)
                    {
                        await MoveFile(s3Object.Key, key + s3Object.Key.Substring(sourceKey.Length), cancellationToken);
                    }
                }
                while (response.IsTruncated);

                return new S3DirectoryEntry(key);
            }

            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            return _client.DeleteObjectAsync(_options.BucketName, ((S3FileSystemEntry)entry).Key, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(
            IUnixDirectoryEntry targetDirectory,
            string directoryName,
            CancellationToken cancellationToken)
        {
            var key = S3Path.Combine(((S3DirectoryEntry)targetDirectory).Key, directoryName + "/");

            await _client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                },
                cancellationToken);

            return new S3DirectoryEntry(key);
        }

        /// <inheritdoc />
        public async Task<Stream> OpenReadAsync(
            IUnixFileEntry fileEntry,
            long startPosition,
            CancellationToken cancellationToken)
        {
            var stream = await _transferUtility.OpenStreamAsync(
                _options.BucketName,
                ((S3FileSystemEntry)fileEntry).Key,
                cancellationToken);
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
            var key = S3Path.Combine(((S3DirectoryEntry)targetDirectory).Key, fileName);
            await UploadFile(data, key, cancellationToken);
            return default;
        }

        /// <inheritdoc />
        public async Task<IBackgroundTransfer?> ReplaceAsync(
            IUnixFileEntry fileEntry,
            Stream data,
            CancellationToken cancellationToken)
        {
            await UploadFile(data, ((S3FileEntry)fileEntry).Key, cancellationToken);
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
                var s3Object = await _client.GetObjectMetadataAsync(
                    new GetObjectMetadataRequest
                    {
                        BucketName = _options.BucketName,
                        Key = key,
                    }, cancellationToken);

                if (key.EndsWith("/"))
                    return new S3DirectoryEntry(key);

                return new S3FileEntry(key, s3Object.Headers.ContentLength)
                {
                    LastWriteTime = s3Object.LastModified,
                };
            }
            catch (AmazonS3Exception)
            {
            }

            return null;
        }

        private async Task<IReadOnlyList<IUnixFileSystemEntry>> ListObjectsAsync(
            string prefix,
            bool includeSelf,
            CancellationToken cancellationToken)
        {
            var objects = new List<IUnixFileSystemEntry>();

            ListObjectsResponse response;
            string? marker = null;
            do
            {
                response = await _client.ListObjectsAsync(
                    new ListObjectsRequest
                    {
                        BucketName = _options.BucketName,
                        Marker = marker,
                        Prefix = prefix,
                        Delimiter = "/",
                    },
                    cancellationToken);

                foreach (var directory in response.CommonPrefixes)
                {
                    objects.Add(new S3DirectoryEntry(directory));
                }

                foreach (var s3Object in response.S3Objects)
                {
                    if (s3Object.Key.EndsWith("/") && s3Object.Key == prefix)
                    {
                        // this is the folder itself
                        if (includeSelf)
                            objects.Add(new S3DirectoryEntry(s3Object.Key));

                        continue;
                    }

                    objects.Add(
                        new S3FileEntry(s3Object.Key, s3Object.Size)
                        {
                            LastWriteTime = s3Object.LastModified,
                        });
                }

                marker = response.NextMarker;
            }
            while (response.IsTruncated);

            return objects;
        }

        private async Task MoveFile(string sourceKey, string key, CancellationToken cancellationToken)
        {
            await _client.CopyObjectAsync(_options.BucketName, sourceKey, _options.BucketName, key, cancellationToken);
            await _client.DeleteObjectAsync(_options.BucketName, sourceKey, cancellationToken);
        }

        private async Task UploadFile(Stream data, string key, CancellationToken cancellationToken)
        {
            var upload = await _client.InitiateMultipartUploadAsync(_options.BucketName, key, cancellationToken);
            try
            {
                var index = 1;
                var buffer = new byte[5 * 1024 * 1024]; // min size for parts is 5MB

                var responses = new List<UploadPartResponse>();

                var chunk = 0;
                do
                {
                    var read = 0;
                    while (read < buffer.Length
                           && (chunk = await data.ReadAsync(buffer, read, buffer.Length - read, cancellationToken)) > 0)
                    {
                        read += chunk; // read till buffer is full
                    }

                    using var ms = new MemoryStream(buffer, 0, read);
                    responses.Add(
                        await _client.UploadPartAsync(
                            new UploadPartRequest
                            {
                                BucketName = _options.BucketName,
                                Key = key,
                                UploadId = upload.UploadId,
                                PartNumber = index++,
                                PartSize = read,
                                InputStream = ms,
                            },
                            cancellationToken));
                }
                while (chunk > 0);

                var request = new CompleteMultipartUploadRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    UploadId = upload.UploadId,
                };
                request.AddPartETags(responses);

                await _client.CompleteMultipartUploadAsync(request, cancellationToken);
            }
            catch (Exception)
            {
                // do not pass cancellation token because this most likely happened because the task was cancelled
                await _client.AbortMultipartUploadAsync(
                    _options.BucketName,
                    key,
                    upload.UploadId,
                    CancellationToken.None);
            }
        }
    }
}
