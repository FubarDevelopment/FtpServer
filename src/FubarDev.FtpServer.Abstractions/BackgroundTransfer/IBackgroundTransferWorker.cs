// <copyright file="IBackgroundTransferWorker.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    /// <summary>
    /// A background transfer worker.
    /// </summary>
    public interface IBackgroundTransferWorker
    {
        /// <summary>
        /// Enqueue an entry for a background transfer (e.g. upload).
        /// </summary>
        /// <param name="backgroundTransfer">The background transfer to enqueue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task EnqueueAsync(IBackgroundTransfer backgroundTransfer, CancellationToken cancellationToken);

        /// <summary>
        /// Get the status of all pending and active background transfers.
        /// </summary>
        /// <returns>The status of all background transfers.</returns>
        IReadOnlyCollection<BackgroundTransferInfo> GetStates();
    }
}
