// <copyright file="IAsyncFtpResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Response that returns the lines asynchronously.
    /// </summary>
    public interface IAsyncFtpResponse : IFtpResponse
    {
        /// <summary>
        /// Gets the async enumeration for all lines to be sent to the client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async enumeration to get all lines.</returns>
        IAsyncEnumerable<string> GetLinesAsync(CancellationToken cancellationToken);
    }
}
