//-----------------------------------------------------------------------
// <copyright file="BackgroundUpload.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using File = RestSharp.Portable.Google.Drive.Model.File;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    internal class BackgroundUpload : IBackgroundTransfer
    {
        private readonly ITemporaryData _tempData;

        private readonly GoogleDriveFileSystem _fileSystem;

        private bool _disposedValue;

        public BackgroundUpload([NotNull] string fullPath, [NotNull] File file, [NotNull] ITemporaryData tempData, [NotNull] GoogleDriveFileSystem fileSystem)
        {
            TransferId = fullPath;
            File = file;
            _tempData = tempData;
            _fileSystem = fileSystem;
        }

        public File File { get; }

        public long FileSize => _tempData.Size;

        public string TransferId { get; }

        public async Task Start(CancellationToken cancellationToken)
        {
            using (var stream = await _tempData.OpenAsync())
            {
                await _fileSystem.Service.UploadAsync(File.Id, stream, "application/octet-stream", cancellationToken);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tempData.Dispose();
                    _fileSystem.UploadFinished(File.Id);
                }
                _disposedValue = true;
            }
        }
    }
}
