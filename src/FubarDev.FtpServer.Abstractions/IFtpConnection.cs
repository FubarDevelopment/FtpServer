//-----------------------------------------------------------------------
// <copyright file="IFtpConnection.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http.Features;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface for an FTP connection.
    /// </summary>
    public interface IFtpConnection
    {
        /// <summary>
        /// Gets or sets the event handler that is triggered when the connection is closed.
        /// </summary>
        event EventHandler? Closed;

        /// <summary>
        /// Gets the connection services.
        /// </summary>
        [Obsolete("Query the IServiceProvidersFeature to get the service provider.")]
        IServiceProvider ConnectionServices { get; }

        /// <summary>
        /// Gets the feature collection.
        /// </summary>
        IFeatureCollection Features { get; }

        /// <summary>
        /// Gets the cancellation token to use to signal a task cancellation.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Starts processing of messages for this connection.
        /// </summary>
        /// <returns>The task.</returns>
        Task StartAsync();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns>The task.</returns>
        Task StopAsync();

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST.
        /// </summary>
        /// <param name="timeout">The timeout for establishing a data connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The data connection.</returns>
        Task<IFtpDataConnection> OpenDataConnectionAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}
