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
        /// Get the the lines to be sent to the client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>All text to be sent to the client.</returns>
        IAsyncEnumerable<string> GetLinesAsync(CancellationToken cancellationToken);
    }
}
