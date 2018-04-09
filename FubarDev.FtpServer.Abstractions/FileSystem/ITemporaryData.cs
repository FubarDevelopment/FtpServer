//-----------------------------------------------------------------------
// <copyright file="ITemporaryData.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// Temporary data holder
    /// </summary>
    /// <remarks>
    /// Temporary data gets removed from the system when it's disposed
    /// </remarks>
    public interface ITemporaryData : IDisposable
    {
        /// <summary>
        /// Gets the size of the temporary data
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Loads the temporary data from a stream
        /// </summary>
        /// <param name="stream">The stream to load the data from</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The task the loading happens on</returns>
        [NotNull]
        Task FillAsync([NotNull] Stream stream, CancellationToken cancellationToken);

        /// <summary>
        /// Opens the temporary data and returns a stream
        /// </summary>
        /// <returns>The stream containig the temporary data</returns>
        [NotNull]
        [ItemNotNull]
        Task<Stream> OpenAsync();
    }
}
