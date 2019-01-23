// <copyright file="GoogleDriveFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.BackgroundTransfer;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// A file system provider for Google Drive.
    /// </summary>
    public class GoogleDriveFileSystemProvider : IFileSystemClassFactory
    {
        [NotNull]
        private readonly IGoogleDriveServiceProvider _serviceProvider;

        [NotNull]
        private readonly ITemporaryDataFactory _temporaryDataFactory;

        [NotNull]
        private readonly GoogleDriveOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveFileSystemProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The google drive service provider.</param>
        /// <param name="temporaryDataFactory">The factory to create temporary data objects.</param>
        /// <param name="options">Options for the Google Drive file system.</param>
        public GoogleDriveFileSystemProvider(
            [NotNull] IGoogleDriveServiceProvider serviceProvider,
            [NotNull] ITemporaryDataFactory temporaryDataFactory,
            [NotNull] IOptions<GoogleDriveOptions> options)
        {
            _serviceProvider = serviceProvider;
            _temporaryDataFactory = temporaryDataFactory;
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var (driveService, rootItem) = await _serviceProvider.GetUserRootAsync(
                accountInformation, CancellationToken.None);
            return new GoogleDriveFileSystem(driveService, rootItem, _temporaryDataFactory, _options.UseBackgroundUpload);
        }
    }
}
