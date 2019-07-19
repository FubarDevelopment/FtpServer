// <copyright file="FtpResponseListBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;

using FubarDev.FtpServer.Utilities;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// An FTP response for lists.
    /// </summary>
    public abstract class FtpResponseListBase : IFtpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseListBase"/> class.
        /// </summary>
        /// <param name="code">The status code.</param>
        protected FtpResponseListBase(
            int code)
        {
            Code = code;
        }

        /// <inheritdoc />
        public int Code { get; }

        /// <inheritdoc />
        [Obsolete("Use a custom server command.")]
        public FtpResponseAfterWriteAsyncDelegate? AfterWriteAction => null;

        /// <inheritdoc />
        public IAsyncEnumerable<string> GetLinesAsync(CancellationToken cancellationToken)
        {
            return GetOutputLines(cancellationToken).ToAsyncEnumerable();
        }

        /// <summary>
        /// Gets the output lines for all source lines.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The lines to send to the client.</returns>
        protected abstract IEnumerable<string> GetOutputLines(CancellationToken cancellationToken);
    }
}
