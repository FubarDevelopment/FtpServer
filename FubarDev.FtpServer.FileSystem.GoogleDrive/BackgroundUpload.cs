//-----------------------------------------------------------------------
// <copyright file="BackgroundUpload.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using File = RestSharp.Portable.Google.Drive.Model.File;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    internal class BackgroundUpload : IBackgroundTransfer
    {
        private readonly string _tempFileName;

        private readonly GoogleDriveFileSystem _fileSystem;

        private bool _disposedValue;

        public BackgroundUpload(string fullPath, File file, string tempFileName, GoogleDriveFileSystem fileSystem, long fileSize)
        {
            TransferId = fullPath;
            File = file;
            _tempFileName = tempFileName;
            _fileSystem = fileSystem;
            FileSize = fileSize;
        }

        public File File { get; }

        public long FileSize { get; }

        public string TransferId { get; }

        public async Task Start(CancellationToken cancellationToken)
        {
            using (var stream = new FileStream(_tempFileName, FileMode.Open))
            {
                await _fileSystem.Service.UploadAsync(File, stream, cancellationToken);
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
                    System.IO.File.Delete(_tempFileName);
                    _fileSystem.UploadFinished(File.Id);
                }
                _disposedValue = true;
            }
        }
    }
}
