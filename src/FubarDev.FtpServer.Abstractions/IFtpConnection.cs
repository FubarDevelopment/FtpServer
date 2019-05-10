//-----------------------------------------------------------------------
// <copyright file="IFtpConnection.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FubarDev.FtpServer.Features;
using JetBrains.Annotations;

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
        event EventHandler Closed;

        /// <summary>
        /// Gets the connection services.
        /// </summary>
        [NotNull]
        IServiceProvider ConnectionServices { get; }

        /// <summary>
        /// Gets the feature collection.
        /// </summary>
        [NotNull]
        IFeatureCollection Features { get; }

        /// <summary>
        /// Gets or sets the encoding for the LIST/NLST commands.
        /// </summary>
        [Obsolete("Query the information using the IEncodingFeature instead.")]
        [NotNull]
        Encoding Encoding { get; set; }

        /// <summary>
        /// Gets the FTP connection data.
        /// </summary>
        [NotNull]
        FtpConnectionData Data { get; }

        /// <summary>
        /// Gets the FTP connection logger.
        /// </summary>
        [CanBeNull]
        ILogger Log { get; }

        /// <summary>
        /// Gets the control connection stream.
        /// </summary>
        [NotNull]
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        Stream OriginalStream { get; }

        /// <summary>
        /// Gets or sets the control connection stream.
        /// </summary>
        [NotNull]
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
        void Start();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Writes a FTP response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the FtpCommandHandler.CommandContext.ResponseWriter or FtpCommandHandlerExtension.CommandContext.ResponseWriter instead.")]
        [NotNull]
        Task WriteAsync([NotNull] IFtpResponse response, CancellationToken cancellationToken);

        /// <summary>
        /// Writes response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [Obsolete("Use the FtpCommandHandler.CommandContext.ResponseWriter or FtpCommandHandlerExtension.CommandContext.ResponseWriter instead.")]
        [NotNull]
        Task WriteAsync([NotNull] string response, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST.
        /// </summary>
        /// <returns>The data connection.</returns>
        [NotNull]
        [ItemNotNull]
        Task<TcpClient> CreateResponseSocket();

        /// <summary>
        /// Create an encrypted stream.
        /// </summary>
        /// <param name="unencryptedStream">The stream to encrypt.</param>
        /// <returns>The encrypted stream.</returns>
        [NotNull]
        [ItemNotNull]
        Task<Stream> CreateEncryptedStream([NotNull] Stream unencryptedStream);
    }
}
