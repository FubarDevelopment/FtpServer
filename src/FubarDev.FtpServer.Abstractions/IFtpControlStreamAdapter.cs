// <copyright file="IFtpControlStreamAdapter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Adapter for the control stream.
    /// </summary>
    public interface IFtpControlStreamAdapter
    {
        /// <summary>
        /// Wraps the control stream.
        /// </summary>
        /// <param name="stream">The stream to wrap.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the new stream.</returns>
        Task<Stream> WrapAsync(Stream stream, CancellationToken cancellationToken);
    }
}
