// <copyright file="GoogleDriveFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;

using JetBrains.Annotations;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveFileSystemProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The google drive service provider.</param>
        /// <param name="temporaryDataFactory">The factory to create temporary data objects.</param>
        public GoogleDriveFileSystemProvider(
            [NotNull] IGoogleDriveServiceProvider serviceProvider,
            [NotNull] ITemporaryDataFactory temporaryDataFactory)
        {
            _serviceProvider = serviceProvider;
            _temporaryDataFactory = temporaryDataFactory;
        }

        /// <inheritdoc />
        public async Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var (driveService, rootItem) = await _serviceProvider.GetUserRootAsync(
                userId, isAnonymous, CancellationToken.None);
            return new GoogleDriveFileSystem(driveService, rootItem, _temporaryDataFactory);
        }
    }
}
