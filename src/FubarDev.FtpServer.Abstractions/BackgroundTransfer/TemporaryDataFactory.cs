// <copyright file="TemporaryDataFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// Factory to create <see cref="ITemporaryData"/> objects.
    /// </summary>
    public class TemporaryDataFactory : ITemporaryDataFactory
    {
        private readonly List<TemporaryDataCreator> _creators = new List<TemporaryDataCreator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryDataFactory"/> class.
        /// </summary>
        public TemporaryDataFactory()
        {
            // In-Memory for small files
            _creators.Add(new TemporaryDataCreator(0, int.MinValue, CreateInMemoryAsync));

            // Temporary file for data sizes > 4MB
            _creators.Add(new TemporaryDataCreator(4194304, int.MinValue, CreateTempFileAsync));
        }

        /// <summary>
        /// The delegate to create temporary data objects.
        /// </summary>
        /// <param name="input">The data for the temporary data objects.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected delegate Task<ITemporaryData> CreateAsyncDelegate(Stream input, CancellationToken cancellationToken);

        /// <inheritdoc />
        public Task<ITemporaryData> CreateAsync(Stream input, long? expectedSize, CancellationToken cancellationToken)
        {
            TemporaryDataCreator creator;
            if (expectedSize == null)
            {
                creator = _creators.OrderByDescending(x => x.MinimumSize).ThenByDescending(x => x.Level).First();
            }
            else
            {
                var filtered = _creators.Where(x => x.MinimumSize <= expectedSize.Value).ToList();
                if (filtered.Count == 0)
                {
                    creator = _creators.OrderBy(x => x.MinimumSize).ThenByDescending(x => x.Level).First();
                }
                else
                {
                    var maxSize = filtered.Max(x => x.MinimumSize);
                    creator = filtered
                        .Where(x => x.MinimumSize == maxSize).OrderByDescending(x => x.Level)
                        .First();
                }
            }

            return creator.CreateAsync(input, cancellationToken);
        }

        /// <summary>
        /// Adds a creator for the given minimum size.
        /// </summary>
        /// <param name="minimumSize">The minimum size required to use the passed creation function.</param>
        /// <param name="createAsyncDelegate">The creation function when the expected size exceeds the minimum size.</param>
        protected void AddCreator(long minimumSize, CreateAsyncDelegate createAsyncDelegate)
        {
            _creators.Add(new TemporaryDataCreator(minimumSize, 0, createAsyncDelegate));
        }

        private static async Task<ITemporaryData> CreateTempFileAsync(
            Stream input,
            CancellationToken cancellationToken)
        {
            var result = new TempFileTemporaryData();
            await result.FillAsync(input, cancellationToken).ConfigureAwait(false);
            return result;
        }

        private static async Task<ITemporaryData> CreateInMemoryAsync(
            Stream input,
            CancellationToken cancellationToken)
        {
            var result = new MemoryTemporaryData();
            await result.FillAsync(input, cancellationToken).ConfigureAwait(false);
            return result;
        }

        private class TemporaryDataCreator
        {
            public TemporaryDataCreator(long minimumSize, int level, CreateAsyncDelegate createAsync)
            {
                MinimumSize = minimumSize;
                Level = level;
                CreateAsync = createAsync;
            }

            public long MinimumSize { get; }
            public int Level { get; }
            public CreateAsyncDelegate CreateAsync { get; }
        }
    }
}
