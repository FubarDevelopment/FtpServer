//-----------------------------------------------------------------------
// <copyright file="IFtpConnection.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface for an FTP connection.
    /// </summary>
    public interface IFtpConnection : IConnectionFeature, IDisposable
    {
        /// <summary>
        /// Gets or sets the event handler that is triggered when the connection is closed.
        /// </summary>
        event EventHandler? Closed;

        /// <summary>
        /// Gets the connection services.
        /// </summary>
        IServiceProvider ConnectionServices { get; }

        /// <summary>
        /// Gets the feature collection.
        /// </summary>
        IFeatureCollection Features { get; }

        /// <summary>
        /// Gets or sets the encoding for the LIST/NLST commands.
        /// </summary>
        [Obsolete("Query the information using the IEncodingFeature instead.")]
        Encoding Encoding { get; set; }

        /// <summary>
        /// Gets the FTP connection data.
        /// </summary>
        [Obsolete("Query the information using the Features property instead.")]
        FtpConnectionData Data { get; }

        /// <summary>
        /// Gets the FTP connection logger.
        /// </summary>
        [Obsolete("Use your own logger instead of the one from the connection.")]
        ILogger? Log { get; }

        /// <summary>
        /// Gets the control connection stream.
        /// </summary>
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        Stream OriginalStream { get; }

        /// <summary>
        /// Gets or sets the control connection stream.
        /// </summary>
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        Stream SocketStream { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a secure connection.
        /// </summary>
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        bool IsSecure { get; }

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
        /// Writes a FTP response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the FtpCommandHandler.CommandContext.ServerCommandWriter or FtpCommandHandlerExtension.CommandContext.ServerCommandWriter instead.")]
        Task WriteAsync(IFtpResponse response, CancellationToken cancellationToken);

        /// <summary>
        /// Writes response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the FtpCommandHandler.CommandContext.ServerCommandWriter or FtpCommandHandlerExtension.CommandContext.ServerCommandWriter instead.")]
        Task WriteAsync(string response, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST.
        /// </summary>
        /// <param name="timeout">The timeout for establishing a data connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The data connection.</returns>
        Task<IFtpDataConnection> OpenDataConnectionAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create an encrypted stream.
        /// </summary>
        /// <param name="unencryptedStream">The stream to encrypt.</param>
        /// <returns>The encrypted stream.</returns>
        [Obsolete("The data connection returned by OpenDataConnection is already encrypted.")]
        Task<Stream> CreateEncryptedStream(Stream unencryptedStream);
    }
}
