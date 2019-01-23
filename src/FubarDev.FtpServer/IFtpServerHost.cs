// <copyright file="IFtpServerHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for a simple FTP server host.
    /// </summary>
    /// <remarks>
    /// This services is used to start and stop all <see cref="IFtpService"/> instances.
    /// </remarks>
    public interface IFtpServerHost
    {
        /// <summary>
        /// Must be called to start the FTP server host.
        /// </summary>
        /// <remarks>
        /// Starts all FTP server services.
        /// </remarks>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>The task.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Must be called for a graceful shutdown of the FTP server host.
        /// </summary>
        /// <remarks>
        /// Stops all FTP server services.
        /// </remarks>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>The task.</returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
