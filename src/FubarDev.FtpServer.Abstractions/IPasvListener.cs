// <copyright file="IPasvListener.cs" company="40three GmbH">
// Copyright (c) 2019 40three GmbH. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Wraps a TCPListener so we can easily use it in our commands that require a passive data connection.
    /// </summary>
    public interface IPasvListener : IDisposable
    {
        /// <summary>
        ///  Gets the Endpoint under which the listener is reachable by clients.
        /// </summary>
        IPEndPoint PasvEndPoint { get; }

        /// <summary>
        /// Accept a client from a PASV command.
        /// </summary>
        /// <returns>A TcpClient with which to communicate with the client.</returns>
        /// <exception cref="SocketException">Network error, see <see cref="SocketError"/>.</exception>
        /// <exception cref="ObjectDisposedException">Listener was disposed of.</exception>
        Task<TcpClient> AcceptPasvClientAsync();
    }
}
