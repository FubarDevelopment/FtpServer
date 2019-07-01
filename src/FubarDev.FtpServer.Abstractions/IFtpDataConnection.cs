// <copyright file="IFtpDataConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An FTP data connection.
    /// </summary>
    public interface IFtpDataConnection
    {
        /// <summary>
        /// Gets the local IP end point.
        /// </summary>
        [NotNull]
        IPEndPoint LocalAddress { get; }

        /// <summary>
        /// Gets the remote IP end point.
        /// </summary>
        [NotNull]
        IPEndPoint RemoteAddress { get; }

        /// <summary>
        /// Gets the stream for this data connection.
        /// </summary>
        [NotNull]
        Stream Stream { get; }

        /// <summary>
        /// Closes the connection (and the stream).
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        [NotNull]
        Task CloseAsync(CancellationToken cancellationToken);
    }
}
