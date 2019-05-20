// <copyright file="TempFileTemporaryData.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// Creates a temporary file to store the data.
    /// </summary>
    public class TempFileTemporaryData : ITemporaryData
    {
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        private const int DefaultCopyBufferSize = 81920;

        private readonly string _tempFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFileTemporaryData"/> class.
        /// </summary>
        public TempFileTemporaryData()
        {
            _tempFileName = Path.GetTempFileName();
        }

        /// <inheritdoc />
        public long Size => new FileInfo(_tempFileName).Length;

        /// <summary>
        /// Fills the temporary file with the data from the input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FillAsync(Stream input, CancellationToken cancellationToken)
        {
            using (var output = new FileStream(_tempFileName, FileMode.Truncate))
            {
                await input.CopyToAsync(output, DefaultCopyBufferSize, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public Task<Stream> OpenAsync()
        {
            return Task.FromResult<Stream>(File.OpenRead(_tempFileName));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            File.Delete(_tempFileName);
        }
    }
}
