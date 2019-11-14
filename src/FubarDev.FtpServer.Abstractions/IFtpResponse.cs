// <copyright file="IFtpResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Base interface to get the response for an FTP command.
    /// </summary>
    public interface IFtpResponse
    {
        /// <summary>
        /// Gets the response code.
        /// </summary>
        int Code { get; }

        /// <summary>
        /// Tries to get the the next line.
        /// </summary>
        /// <param name="token">Token that saves the current position. Must be <see langword="null"/> at the beginning.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see langword="true"/> when a new line is available to send.</returns>
        [Obsolete("Use IAsyncFtpResponse.GetLinesAsync instead.")]
        Task<FtpResponseLine> GetNextLineAsync(object? token, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the async enumeration for all lines to be sent to the client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async enumeration to get all lines.</returns>
        IAsyncEnumerable<string> GetLinesAsync(CancellationToken cancellationToken);
    }
}
