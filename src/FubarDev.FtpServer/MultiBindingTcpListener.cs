// <copyright file="MultiBindingTcpListener.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Allows binding to a host name, which in turn may resolve to multiple IP addresses.
    /// </summary>
    public class MultiBindingTcpListener
    {
        private readonly string _address;
        private readonly int _port;
        [CanBeNull]
        private readonly ILogger _logger;
        private readonly IList<TcpListener> _listeners = new List<TcpListener>();
        private readonly IList<Task<TcpClient>> _acceptors = new List<Task<TcpClient>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiBindingTcpListener"/> class.
        /// </summary>
        /// <param name="address">The address/host name to bind to.</param>
        /// <param name="port">The listener port.</param>
        /// <param name="logger">The logger.</param>
        public MultiBindingTcpListener(string address, int port, [CanBeNull] ILogger logger)
        {
            if (port < 1 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "The port argument is out of range");
            }

            _address = address;
            _port = port;
            _logger = logger;
        }

        /// <summary>
        /// Gets the port this listener is bound to.
        /// </summary>
        public int? Port { get; private set; }

        /// <summary>
        /// Start all listeners.
        /// </summary>
        /// <returns>the task.</returns>
        public async Task StartAsync()
        {
            _logger?.LogDebug("Server configured for listening on {address}:{port}", _address, _port);

            List<IPAddress> addresses;
            if (string.IsNullOrEmpty(_address) || _address == IPAddress.Any.ToString())
            {
                // "0.0.0.0"
                addresses = new List<IPAddress> { IPAddress.Any };
            }
            else if (_address == IPAddress.IPv6Any.ToString())
            {
                // "::"
                addresses = new List<IPAddress> { IPAddress.IPv6Any };
            }
            else if (_address == "*")
            {
                addresses = new List<IPAddress> { IPAddress.Any, IPAddress.IPv6Any };
            }
            else
            {
                var dnsAddresses = await Dns.GetHostAddressesAsync(_address).ConfigureAwait(false);
                addresses = dnsAddresses
                    .Where(x => x.AddressFamily == AddressFamily.InterNetwork ||
                                x.AddressFamily == AddressFamily.InterNetworkV6)
                    .ToList();
            }

            try
            {
                Port = StartListening(addresses, _port);
            }
            catch
            {
                Stop();
                throw;
            }
        }

        /// <summary>
        /// Stops all listeners.
        /// </summary>
        public void Stop()
        {
            foreach (var listener in _listeners)
            {
                listener.Stop();
            }

            _listeners.Clear();
            Port = 0;
        }

        /// <summary>
        /// Tries to get a listener that has pending client connections.
        /// </summary>
        /// <param name="listener">The listener that has pending client connections.</param>
        /// <returns><c>true</c> when a listener with pending client connections could be found.</returns>
        public bool TryGetPending(out TcpListener listener)
        {
            foreach (var tcpListener in _listeners)
            {
                if (tcpListener.Pending())
                {
                    listener = tcpListener;
                    return true;
                }
            }

            listener = null;
            return false;
        }

        /// <summary>
        /// Wait for any client on all listeners.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The new TCP client.</returns>
        public Task<TcpClient> WaitAnyTcpClientAsync(CancellationToken token)
        {
            var index = Task.WaitAny(_acceptors.Cast<Task>().ToArray(), token);
            var retVal = _acceptors[index];
            _acceptors[index] = _listeners[index].AcceptTcpClientAsync();
            return retVal;
        }

        /// <summary>
        /// Start the asynchronous acception for all listeners.
        /// </summary>
        public void StartAccepting()
        {
            _listeners.ToList().ForEach(x => _acceptors.Add(x.AcceptTcpClientAsync()));
        }

        private int StartListening(IEnumerable<IPAddress> addresses, int port)
        {
            var selectedPort = port;
            foreach (var address in addresses)
            {
                var listener = new TcpListener(address, selectedPort);
                listener.Start();

                if (selectedPort == 0)
                {
                    selectedPort = ((IPEndPoint)listener.LocalEndpoint).Port;
                }

                _logger?.LogInformation("Started listening on {address}:{port}", address, selectedPort);

                _listeners.Add(listener);
            }

            return selectedPort;
        }
    }
}
