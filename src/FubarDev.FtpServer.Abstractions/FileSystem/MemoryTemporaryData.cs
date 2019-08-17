//-----------------------------------------------------------------------
// <copyright file="MemoryTemporaryData.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Stores the temporary data in a memory stream.
    /// </summary>
    /// <remarks>
    /// This is not recommended in a production environment.
    /// </remarks>
    public class MemoryTemporaryData : ITemporaryData
    {
        private bool _disposedValue;

        private MemoryStream? _data;

        /// <inheritdoc/>
        public long Size => _data?.Length ?? 0;

        /// <summary>
        /// Copies the <paramref name="stream"/> data into the temporary data store.
        /// </summary>
        /// <param name="stream">The data to copy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public async Task FillAsync(Stream stream, CancellationToken cancellationToken)
        {
            _data?.Dispose();
            _data = new MemoryStream();
            await stream.CopyToAsync(_data, 4096, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<Stream> OpenAsync()
        {
            if (_data == null)
            {
                _data = new MemoryStream();
            }
            else
            {
                _data.Position = 0;
            }

            return Task.FromResult<Stream>(_data);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing"><code>true</code> when called from the <see cref="Dispose()"/> function.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _data?.Dispose();
                    _data = null;
                }

                _disposedValue = true;
            }
        }
    }
}
