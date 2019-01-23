// <copyright file="IBackgroundTransferWorker.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

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
        void Enqueue([NotNull] IBackgroundTransfer backgroundTransfer);

        /// <summary>
        /// Get the status of all enqueued and active background transfers.
        /// </summary>
        /// <returns>The status of all background transfers.</returns>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<BackgroundTransferInfo> GetStates();
    }
}
