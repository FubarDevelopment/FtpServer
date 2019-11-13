// <copyright file="IFtpDataConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
        IPEndPoint LocalAddress { get; }

        /// <summary>
        /// Gets the remote IP end point.
        /// </summary>
        IPEndPoint RemoteAddress { get; }

        /// <summary>
        /// Gets the stream for this data connection.
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// Gets a value indicating whether the connection was closed.
        /// </summary>
        bool Closed { get; }

        /// <summary>
        /// Closes the connection (and the stream).
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task CloseAsync(CancellationToken cancellationToken);
    }
}
