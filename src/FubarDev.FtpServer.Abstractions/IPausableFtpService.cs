// <copyright file="IPausableFtpService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for an FTP service that can be paused.
    /// </summary>
    public interface IPausableFtpService : IFtpService
    {
        /// <summary>
        /// Gets the current status.
        /// </summary>
        FtpServiceStatus Status { get; }

        /// <summary>
        /// Pauses the FTP service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task PauseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Continues the FTP service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task ContinueAsync(CancellationToken cancellationToken);
    }
}
