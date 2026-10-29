// <copyright file="S3FileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.FileSystem.AzureBlobStorage
{
    /// <summary>
    /// The file system factory for a S3-based file system.
    /// </summary>
    internal class AzureBlobStorageFileSystemProvider : IFileSystemClassFactory
    {
        private readonly AzureBlobStorageFileSystemOptions _options;
        private readonly IAccountDirectoryQuery _accountDirectoryQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        /// <param name="accountDirectoryQuery">Interface to query account directories.</param>
        /// <exception cref="ArgumentException">Gets thrown when the azure blob storage credentials weren't set.</exception>
        public AzureBlobStorageFileSystemProvider(IOptions<AzureBlobStorageFileSystemOptions> options, IAccountDirectoryQuery accountDirectoryQuery)
        {
            _options = options.Value;
            _accountDirectoryQuery = accountDirectoryQuery;

            if (string.IsNullOrEmpty(_options.ContainerName)
                || string.IsNullOrEmpty(_options.ConnectionString))
            {
                throw new ArgumentException("Azure blob storage Credentials have not been set correctly");
            }
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var directories = _accountDirectoryQuery.GetDirectories(accountInformation);

            return Task.FromResult<IUnixFileSystem>(
                new AzureBlobStorageFileSystem(_options, AzureBlobStoragePath.Combine(_options.RootPath ?? "", directories.RootPath)));
        }
    }
}
