//-----------------------------------------------------------------------
// <copyright file="BackgroundUpload.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using File = RestSharp.Portable.Google.Drive.Model.File;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// This class performs the upload in the background
    /// </summary>
    internal class BackgroundUpload : IBackgroundTransfer
    {
        private readonly ITemporaryData _tempData;

        private readonly GoogleDriveFileSystem _fileSystem;

        private bool _notifiedAsFinished;

        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundUpload"/> class.
        /// </summary>
        /// <param name="fullPath">The full path to this item</param>
        /// <param name="file">The file to upload to</param>
        /// <param name="tempData">The temporary data used to read from</param>
        /// <param name="fileSystem">The file system that initiated this background upload</param>
        public BackgroundUpload([NotNull] string fullPath, [NotNull] File file, [NotNull] ITemporaryData tempData, [NotNull] GoogleDriveFileSystem fileSystem)
        {
            TransferId = fullPath;
            File = file;
            _tempData = tempData;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Gets the file to upload to
        /// </summary>
        public File File { get; }

        /// <summary>
        /// Gets the file size
        /// </summary>
        public long FileSize => _tempData.Size;

        /// <inheritdoc/>
        public string TransferId { get; }

        /// <inheritdoc/>
        public async Task Start(CancellationToken cancellationToken)
        {
            using (var stream = await _tempData.OpenAsync())
            {
                try
                {
                    await _fileSystem.Service.UploadAsync(File.Id, stream, "application/octet-stream", cancellationToken);
                }
                finally
                {
                    _notifiedAsFinished = true;
                    _fileSystem.UploadFinished(File.Id);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
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
                    _tempData.Dispose();
                    if (!_notifiedAsFinished)
                        _fileSystem.UploadFinished(File.Id);
                }
                _disposedValue = true;
            }
        }
    }
}
