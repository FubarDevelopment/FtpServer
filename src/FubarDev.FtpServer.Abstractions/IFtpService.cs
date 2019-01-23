// <copyright file="IFtpService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Defines methods for services required to run the FTP server.
    /// </summary>
    public interface IFtpService
    {
        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>The task.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>The task.</returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
