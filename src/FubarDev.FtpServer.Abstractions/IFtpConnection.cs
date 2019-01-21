//-----------------------------------------------------------------------
// <copyright file="IFtpConnection.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface for an FTP connection.
    /// </summary>
    public interface IFtpConnection : IDisposable
    {
        /// <summary>
        /// Gets or sets the event handler that is triggered when the connection is closed.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Gets the dictionary of all known command handlers.
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, IFtpCommandHandler> CommandHandlers { get; }

        /// <summary>
        /// Gets or sets the encoding for the LIST/NLST commands.
        /// </summary>
        [NotNull]
        Encoding Encoding { get; set; }

        /// <summary>
        /// Gets a value indicating whether to accept PASV connections from any source.
        /// If false (default), connections to a PASV port will only be accepted from the same IP that issued
        /// the respective PASV command.
        /// </summary>
        bool PromiscuousPasv { get; }

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
        /// Gets the local end point.
        /// </summary>
        [NotNull]
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the control connection stream.
        /// </summary>
        [NotNull]
        Stream OriginalStream { get; }

        /// <summary>
        /// Gets or sets the control connection stream.
        /// </summary>
        [NotNull]
        Stream SocketStream { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a secure connection.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Gets the remote address of the client.
        /// </summary>
        [NotNull]
        Address RemoteAddress { get; }

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
        Task WriteAsync([NotNull] FtpResponse response, CancellationToken cancellationToken);

        /// <summary>
        /// Writes response to a client.
        /// </summary>
        /// <param name="response">The response to write to the client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task WriteAsync([NotNull] string response, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a response socket for e.g. LIST/NLST.
        /// </summary>
        /// <returns>The data connection.</returns>
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
