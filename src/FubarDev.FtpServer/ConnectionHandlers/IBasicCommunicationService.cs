// <copyright file="IBasicCommunicationService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Interface for a basic communication service.
    /// </summary>
    public interface IBasicCommunicationService
    {
        /// <summary>
        /// Starts the communication service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the communication service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        Task StopAsync(CancellationToken cancellationToken);
    }
}
