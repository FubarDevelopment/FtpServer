// <copyright file="S3FileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.FileSystem.S3
{
    /// <summary>
    /// The file system factory for a S3-based file system.
    /// </summary>
    internal class S3FileSystemProvider : IFileSystemClassFactory
    {
        private readonly S3FileSystemOptions _options;
        private readonly IAccountDirectoryQuery _accountDirectoryQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3FileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        /// <param name="accountDirectoryQuery">Interface to query account directories.</param>
        /// <exception cref="ArgumentException">Gets thrown when the S3 credentials weren't set.</exception>
        public S3FileSystemProvider(IOptions<S3FileSystemOptions> options, IAccountDirectoryQuery accountDirectoryQuery)
        {
            _options = options.Value;
            _accountDirectoryQuery = accountDirectoryQuery;

            if (string.IsNullOrEmpty(_options.AwsAccessKeyId)
                || string.IsNullOrEmpty(_options.AwsSecretAccessKey)
                || string.IsNullOrEmpty(_options.BucketName)
                || string.IsNullOrEmpty(_options.BucketRegion))
            {
                throw new ArgumentException("S3 Credentials have not been set correctly");
            }
        }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var directories = _accountDirectoryQuery.GetDirectories(accountInformation);

            return Task.FromResult<IUnixFileSystem>(
                new S3FileSystem(_options, S3Path.Combine(_options.RootPath, directories.RootPath)));
        }
    }
}
