// <copyright file="IPasvListenerFactory.cs" company="40three GmbH">
// Copyright (c) 2019 40three GmbH. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Creates correctly configured <see cref="IPasvListener">PASV data connection listeners</see>.
    /// </summary>
    public interface IPasvListenerFactory
    {
        /// <summary>
        /// Create a new TcpListener for the given connection.
        /// </summary>
        /// <param name="connection">connection on which to create the tcp listener.</param>
        /// <param name="addressFamily">The address family for the address to be selected.</param>
        /// <param name="port">listen on the given port, or 0 for any port.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="SocketException">Network error, such as no free port.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The chosen port was not within the configured range of ports.</exception>
        /// <returns>A TcpListener.</returns>
        Task<IPasvListener> CreateTcpListenerAsync(
            IFtpConnection connection,
            AddressFamily? addressFamily = null,
            int port = 0,
            CancellationToken cancellationToken = default);
    }
}
