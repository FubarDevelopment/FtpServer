// <copyright file="ITemporaryDataFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// Factory for temporary data objects.
    /// </summary>
    public interface ITemporaryDataFactory
    {
        /// <summary>
        /// Creates a temporary data object for data with the expected size.
        /// </summary>
        /// <param name="input">The stream containing the data.</param>
        /// <param name="expectedSize">The expected data size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created temporary data object.</returns>
        Task<ITemporaryData> CreateAsync(Stream input, long? expectedSize, CancellationToken cancellationToken);
    }
}
