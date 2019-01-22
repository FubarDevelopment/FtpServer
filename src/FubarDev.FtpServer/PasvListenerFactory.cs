// <copyright file="PasvListenerFactory.cs" company="40three GmbH">
// Copyright (c) 2019 40three GmbH. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Creates TcpListeners for use with PASV commands.
    /// </summary>
    public class PasvListenerFactory : IPasvListenerFactory
    {
        private readonly ILogger<PasvListenerFactory> _log;

        private readonly int _pasvMinPort;
        private readonly int _pasvMaxPort;

        private readonly int[] _pasvPorts;

        private readonly Random _prng = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="PasvListenerFactory"/> class.
        /// </summary>
        /// <param name="serverOptions">FTPServer options.</param>
        /// <param name="logger">Logger instance.</param>
        public PasvListenerFactory(IOptions<FtpServerOptions> serverOptions, ILogger<PasvListenerFactory> logger)
        {
            _log = logger;
            if (serverOptions.Value.PasvMinPort > 1023 &&
                serverOptions.Value.PasvMaxPort >= serverOptions.Value.PasvMinPort)
            {
                _pasvMinPort = serverOptions.Value.PasvMinPort;
                _pasvMaxPort = serverOptions.Value.PasvMaxPort;
                _log.LogInformation($"PASV port range set to {_pasvMinPort}:{_pasvMaxPort}");
                _pasvPorts = Enumerable.Range(_pasvMinPort, _pasvMaxPort - _pasvMinPort + 1).ToArray();
            }

            PasvExternalAddress = !string.IsNullOrWhiteSpace(serverOptions.Value.PasvAddress)
                ? IPAddress.Parse(serverOptions.Value.PasvAddress)
                : null;
        }

        /// <summary>
        /// Gets the IP address where clients should direct PASV connection attempts. If null, the control connection
        /// interface's IP is used.
        /// </summary>
        protected IPAddress PasvExternalAddress { get; }

        /// <inheritdoc />
        public Task<IPasvListener> CreateTcpListener(IFtpConnection connection, int port)
        {
            IPasvListener listener;

            if (port < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "may not be less than 0");
            }

            if (port > 0 && _pasvMinPort > 0 && (port > _pasvMaxPort || port < _pasvMinPort))
            {
                throw new ArgumentOutOfRangeException(nameof(port), $"must be in {_pasvMinPort}:{_pasvMaxPort}");
            }

            if (port == 0 && _pasvPorts != null)
            {
                listener = CreateListenerInRange(connection);
            }
            else
            {
                listener = new PasvListener(connection.LocalEndPoint.Address, port, PasvExternalAddress);
            }

            return Task.FromResult(listener);
        }

        /// <summary>
        /// Gets a listener on a port within the assigned port range.
        /// </summary>
        /// <param name="connection">Connection for which to create the listener.</param>
        /// <returns>Configured PasvListener.</returns>
        /// <exception cref="SocketException">When no free port could be found, or other bad things happen. See <see cref="SocketError"/>.</exception>
        private IPasvListener CreateListenerInRange(IFtpConnection connection)
        {
            lock (_pasvPorts)
            {
                // randomize ports so we don't always get the ports in the same order
                var randomizedPorts = _pasvPorts.OrderBy(_ => _prng.Next());
                foreach (var port in randomizedPorts)
                {
                    try
                    {
                        return new PasvListener(connection.LocalEndPoint.Address, port, PasvExternalAddress);
                    }
                    catch (SocketException se)
                    {
                        // retry if the socket is already in use, else throw the underlying exception
                        if (se.SocketErrorCode != SocketError.AddressAlreadyInUse)
                        {
                            _log.LogError(se, "Could not create listener");
                            throw;
                        }
                    }
                }

                // if we reach this point, we have not been able to create a listener within range
                _log.LogWarning("No free ports available for data connection");
                throw new SocketException((int)SocketError.AddressAlreadyInUse);
            }
        }
    }
}
