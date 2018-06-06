// <copyright file="IBackgroundTransferHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.FtpServer.BackgroundTransfer;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface to be implemented by a host for background transfers
    /// </summary>
    public interface IBackgroundTransferHost
    {
        /// <summary>
        /// Enqueue a new <see cref="IBackgroundTransfer"/> for the given <paramref name="connection"/>.
        /// </summary>
        /// <param name="backgroundTransfer">The background transfer to enqueue.</param>
        /// <param name="connection">The connection to enqueue the background transfer for.</param>
        void EnqueueBackgroundTransfer(
            [NotNull] IBackgroundTransfer backgroundTransfer,
            [CanBeNull] IFtpConnection connection);

        /// <summary>
        /// Get the background transfer states for all active <see cref="IBackgroundTransfer"/> objects.
        /// </summary>
        /// <returns>The background transfer states for all active <see cref="IBackgroundTransfer"/> objects.</returns>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<BackgroundTransferInfo> GetBackgroundTaskStates();
    }
}
