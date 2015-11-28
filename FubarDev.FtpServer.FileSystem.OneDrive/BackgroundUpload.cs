//-----------------------------------------------------------------------
// <copyright file="BackgroundUpload.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using RestSharp.Portable.Microsoft.OneDrive.Model;

namespace FubarDev.FtpServer.FileSystem.OneDrive
{
    internal class BackgroundUpload : IBackgroundTransfer
    {
        private readonly ITemporaryData _tempData;

        private readonly OneDriveFileSystem _fileSystem;

        private bool _notifiedAsFinished;

        private bool _disposedValue;

        public BackgroundUpload([NotNull] string fullPath, [NotNull] string parentId, [NotNull] string name, [NotNull] ITemporaryData tempData, [NotNull] OneDriveFileSystem fileSystem)
        {
            TransferId = fullPath;
            ParentId = parentId;
            Name = name;
            _tempData = tempData;
            _fileSystem = fileSystem;
            var now = DateTimeOffset.Now;
            Item = new Item
            {
                Name = Name,
                Size = _tempData.Size,
                LastModifiedDateTime = now,
                CreatedDateTime = now,
                ParentReference = new ItemReference
                {
                    Id = ParentId,
                },
                Folder = new Folder() { ChildCount = 0 },
            };
        }

        public long FileSize => _tempData.Size;

        [NotNull]
        public string ParentId { get; }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public string TransferId { get; }

        [NotNull]
        public Item Item { get; private set; }

        [CanBeNull]
        public Item ItemChanges { get; set; }

        public async Task Start(CancellationToken cancellationToken)
        {
            using (var stream = await _tempData.OpenAsync())
            {
                bool withError = false;
                try
                {
                    Item = await _fileSystem.Service.UploadFileAsync(_fileSystem.Drive.Id, ParentId, Name, stream, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    withError = true;
                    Item.Size = stream.Position;
                    throw;
                }
                finally
                {
                    _notifiedAsFinished = true;
                    await _fileSystem.UploadFinished(ParentId, Name, withError);
                }
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
                    if (!_notifiedAsFinished)
                        _fileSystem.UploadFinished(ParentId, Name, true).Wait();
                }
                _disposedValue = true;
            }
        }
    }
}
