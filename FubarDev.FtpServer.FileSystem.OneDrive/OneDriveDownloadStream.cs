//-----------------------------------------------------------------------
// <copyright file="OneDriveDownloadStream.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using RestSharp.Portable.Microsoft.OneDrive.Model;

namespace FubarDev.FtpServer.FileSystem.OneDrive
{
    internal class OneDriveDownloadStream : Stream
    {
        private readonly HttpWebResponse _response;

        private readonly long _startPosition;

        private readonly Stream _responseStream;

        private readonly OneDriveFileSystem _fileSystem;

        private readonly Item _item;

        private long _position;

        private bool _disposedValue;

        public OneDriveDownloadStream(OneDriveFileSystem fileSystem, Item item, HttpWebResponse response, long startPosition, long contentLength)
        {
            _item = item;
            _fileSystem = fileSystem;
            _startPosition = startPosition;
            _response = response;
            _responseStream = response.GetResponseStream();
            Length = contentLength;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        { get; }

        public override long Position
        {
            get
            {
                return _startPosition + _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readCount = _responseStream.Read(buffer, offset, count);
            _position += readCount;
            if (readCount == 0)
                _fileSystem.DownloadFinished(_item);
            return readCount;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readCount = await _responseStream.ReadAsync(buffer, offset, count, cancellationToken);
            _position += readCount;
            if (readCount == 0)
                _fileSystem.DownloadFinished(_item);
            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _responseStream.Dispose();
                    _response?.Dispose();
                }
                _disposedValue = true;
            }
        }
    }
}
