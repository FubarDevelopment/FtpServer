// <copyright file="PasvListener.cs" company="40three GmbH">
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
    /// The default implementation of the <see cref="IPasvListener"/> interface.
    /// </summary>
    public class PasvListener : IPasvListener
    {
        private readonly TcpListener _tcpListener;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasvListener"/> class.
        /// </summary>
        /// <param name="listenAddress">On which address to listen.</param>
        /// <param name="port">Port to listen on, or 0 for any.</param>
        /// <param name="externalAddress">which external address should be advertised to clients. Use null to use the listener's address.</param>
        /// <exception cref="ArgumentNullException">listenAddress is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">port is out of range.</exception>
        public PasvListener(IPAddress listenAddress, int port, IPAddress externalAddress)
        {
            _tcpListener = new TcpListener(listenAddress, port);
            _tcpListener.Start();

            var listenerEndpoint = (IPEndPoint)_tcpListener.LocalEndpoint;

            PasvEndPoint = (externalAddress == null)
                ? listenerEndpoint
                : new IPEndPoint(externalAddress, listenerEndpoint.Port);
        }

        /// <inheritdoc />
        public IPEndPoint PasvEndPoint { get; }

        /// <inheritdoc />
        public Task<TcpClient> AcceptPasvClientAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("AcceptPasvClientAsync called on disposed PasvListener");
            }

            return _tcpListener.AcceptTcpClientAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _tcpListener.Stop();
            }
        }
    }
}
