//-----------------------------------------------------------------------
// <copyright file="GoogleDriveFileSystem.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RestSharp.Portable.Google.Drive;

using File = RestSharp.Portable.Google.Drive.Model.File;

namespace FubarDev.FtpServer.FileSystem.GoogleDrive
{
    public class GoogleDriveFileSystem : IUnixFileSystem
    {
        private readonly Dictionary<string, BackgroundUpload> _uploads = new Dictionary<string, BackgroundUpload>();

        private readonly SemaphoreSlim _uploadsLock = new SemaphoreSlim(1);

        private bool _disposedValue;

        public GoogleDriveFileSystem(GoogleDriveService service, File rootFolderInfo)
        {
            Service = service;
            RootFolderInfo = rootFolderInfo;
            Root = new GoogleDriveDirectoryEntry(RootFolderInfo, "/", true);
        }

        public GoogleDriveService Service { get; }

        public File RootFolderInfo { get; }

        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;

        public IUnixDirectoryEntry Root { get; }

        public async Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)directoryEntry;
            var entries = await ConvertEntries(
                dirEntry,
                () => Service.GetChildrenAsync(dirEntry.File, cancellationToken),
                cancellationToken);
            return entries;
        }

        public async Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)directoryEntry;
            var entries = await ConvertEntries(
                dirEntry,
                () => Service.FindChildByNameAsync(dirEntry.File, name, cancellationToken),
                cancellationToken);
            return entries.FirstOrDefault();
        }

        public async Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            var parentEntry = (GoogleDriveDirectoryEntry)parent;
            var targetEntry = (GoogleDriveDirectoryEntry)target;
            var targetName = FileSystemExtensions.CombinePath(targetEntry.FullName, fileName);

            var sourceFileEntry = source as GoogleDriveFileEntry;
            if (sourceFileEntry != null)
            {
                var newFile = await Service.MoveAsync(sourceFileEntry.File, parentEntry.File.Id, targetEntry.File, fileName, cancellationToken);
                return new GoogleDriveFileEntry(newFile, targetName);
            }

            var sourceDirEntry = (GoogleDriveDirectoryEntry)source;
            var newDir = await Service.MoveAsync(sourceDirEntry.File, parentEntry.File.Id, targetEntry.File, fileName, cancellationToken);
            return new GoogleDriveDirectoryEntry(newDir, targetName);
        }

        public async Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            var dirEntry = entry as GoogleDriveDirectoryEntry;
            if (dirEntry != null)
            {
                await Service.TrashAsync(dirEntry.File, cancellationToken);
            }
            else
            {
                var fileEntry = (GoogleDriveFileEntry)entry;
                await Service.TrashAsync(fileEntry.File, cancellationToken);
            }
        }

        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            var dirEntry = (GoogleDriveDirectoryEntry)targetDirectory;
            var newDir = await Service.CreateDirectoryAsync(dirEntry.File, directoryName, cancellationToken);
            return new GoogleDriveDirectoryEntry(newDir, FileSystemExtensions.CombinePath(dirEntry.FullName, newDir.Title));
        }

        public async Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            HttpRange range = startPosition != 0 ? new HttpRange("bytes", new HttpRangeItem(startPosition, fileEntry.Size)) : null;
            var fe = (GoogleDriveFileEntry)fileEntry;
            var response = await Service.GetDownloadResponseAsync(fe.File, range, cancellationToken);
            return new GoogleDriveDownloadStream(response, startPosition, fe.Size);
        }

        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            var targetEntry = (GoogleDriveDirectoryEntry)targetDirectory;
            var newFileEntry = await Service.CreateItemAsync(targetEntry.File, fileName, "application/octet-stream", cancellationToken);
            var tempFileName = Path.GetTempFileName();
            long fileSize;
            using (var temp = new FileStream(tempFileName, FileMode.Truncate))
            {
                await data.CopyToAsync(temp, 4096, cancellationToken);
                fileSize = temp.Position;
            }
            var fullPath = FileSystemExtensions.CombinePath(targetEntry.FullName, fileName);
            var backgroundUploads = new BackgroundUpload(fullPath, newFileEntry, tempFileName, this, fileSize);
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                _uploads.Add(backgroundUploads.File.Id, backgroundUploads);
            }
            finally
            {
                _uploadsLock.Release();
            }
            return backgroundUploads;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task<IReadOnlyList<IUnixFileSystemEntry>> ConvertEntries(GoogleDriveDirectoryEntry dirEntry, Func<Task<IReadOnlyList<File>>> getEntriesFunc, CancellationToken cancellationToken)
        {
            var result = new List<IUnixFileSystemEntry>();
            await _uploadsLock.WaitAsync(cancellationToken);
            try
            {
                var baseDir = dirEntry.FullName;
                foreach (var child in (await getEntriesFunc()).Where(x => x.Labels == null || !x.Labels.Trashed))
                {
                    var fullName = FileSystemExtensions.CombinePath(baseDir, child.Title);
                    if (child.IsDirectory())
                    {
                        result.Add(new GoogleDriveDirectoryEntry(child, fullName));
                    }
                    else
                    {
                        long? fileSize;
                        BackgroundUpload uploader;
                        if (_uploads.TryGetValue(child.Id, out uploader))
                        {
                            fileSize = uploader.FileSize;
                        }
                        else
                        {
                            fileSize = null;
                        }
                        result.Add(new GoogleDriveFileEntry(child, fullName, fileSize));
                    }
                }
            }
            finally
            {
                _uploadsLock.Release();
            }
            return result;
        }

        internal void UploadFinished(string fileId)
        {
            _uploadsLock.Wait();
            try
            {
                _uploads.Remove(fileId);
            }
            finally
            {
                _uploadsLock.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Service.Dispose();
                    _uploadsLock.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
