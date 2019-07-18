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
        /// Gets the async action to execute after sending the response to the client.
        /// </summary>
        [Obsolete("Use a custom server command.")]
        FtpResponseAfterWriteAsyncDelegate? AfterWriteAction { get; }

        /// <summary>
        /// Tries to get the the next line.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>All text to be sent to the client.</returns>
        IAsyncEnumerable<string> GetNextLineAsync(CancellationToken cancellationToken);
    }
}
