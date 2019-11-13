// <copyright file="IFtpDataConnectionFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Feature for an FTP data connection.
    /// </summary>
    public interface IFtpDataConnectionFeature
    {
        /// <summary>
        /// Gets the FTP command that initiated the creation of the feature.
        /// </summary>
        FtpCommand? Command { get; }

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        /// <remarks>
        /// This value is unreliable in case of an active data connection.
        /// </remarks>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets a new FTP data connection.
        /// </summary>
        /// <param name="timeout">The timeout for establishing the FTP data connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task<IFtpDataConnection> GetDataConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Async disposing of the handler.
        /// </summary>
        /// <returns>The task.</returns>
        Task DisposeAsync();
    }
}
