// <copyright file="BackgroundUpload.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;

using Google.Apis.Upload;

using File = Google.Apis.Drive.v3.Data.File;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    /// <summary>
    /// This class performs the upload in the background.
    /// </summary>
    internal class BackgroundUpload : IBackgroundTransfer
    {
        private readonly ITemporaryData _tempData;
        private readonly IGoogleDriveFileSystem _fileSystem;

        private bool _notifiedAsFinished;

        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundUpload"/> class.
        /// </summary>
        /// <param name="fullPath">The full path to this item.</param>
        /// <param name="file">The file to upload to.</param>
        /// <param name="tempData">The temporary data used to read from.</param>
        /// <param name="fileSystem">The file system that initiated this background upload.</param>
        public BackgroundUpload(
            string fullPath,
            File file,
            ITemporaryData tempData,
            IGoogleDriveFileSystem fileSystem)
        {
            TransferId = fullPath;
            File = file;
            _tempData = tempData;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Gets the file to upload to.
        /// </summary>
        public File File { get; }

        /// <summary>
        /// Gets the file size.
        /// </summary>
        public long FileSize => (File.Size ?? 0) + _tempData.Size;

        /// <inheritdoc />
        public string TransferId { get; }

        /// <inheritdoc />
        public async Task Start(IProgress<long> progress, CancellationToken cancellationToken)
        {
            using (var stream = await _tempData.OpenAsync().ConfigureAwait(false))
            {
                try
                {
                    var upload = _fileSystem.Service.Files.Update(
                        new File(),
                        File.Id,
                        stream,
                        "application/octet-stream");
                    upload.ProgressChanged += (uploadProgress) => { progress.Report(uploadProgress.BytesSent); };
                    var result = await upload.UploadAsync(cancellationToken).ConfigureAwait(false);
                    if (result.Status == UploadStatus.Failed)
                    {
                        throw new IOException(result.Exception.Message, result.Exception);
                    }
                }
                finally
                {
                    _notifiedAsFinished = true;
                    _fileSystem.UploadFinished(File.Id);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing"><code>true</code> when called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tempData.Dispose();
                    if (!_notifiedAsFinished)
                    {
                        _fileSystem.UploadFinished(File.Id);
                    }
                }
                _disposedValue = true;
            }
        }
    }
}
