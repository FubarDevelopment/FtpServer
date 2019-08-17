// <copyright file="PasvListenerFactory.cs" company="40three GmbH">
// Copyright (c) 2019 40three GmbH. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Creates TcpListeners for use with PASV commands.
    /// </summary>
    public class PasvListenerFactory : IPasvListenerFactory
    {
        private readonly IPasvAddressResolver _addressResolver;
        private readonly ILogger<PasvListenerFactory>? _log;
        private readonly Random _prng = new Random();
        private readonly object _listenerLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PasvListenerFactory"/> class.
        /// </summary>
        /// <param name="addressResolver">The address resolver for <c>PASV</c>/<c>EPSV</c>.</param>
        /// <param name="logger">Logger instance.</param>
        public PasvListenerFactory(IPasvAddressResolver addressResolver, ILogger<PasvListenerFactory>? logger = null)
        {
            _addressResolver = addressResolver;
            _log = logger;
        }

        /// <inheritdoc />
        public async Task<IPasvListener> CreateTcpListenerAsync(
            IFtpConnection connection,
            AddressFamily? addressFamily,
            int port,
            CancellationToken cancellationToken)
        {
            IPasvListener listener;

            if (port < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "may not be less than 0");
            }

            var pasvOptions = await _addressResolver.GetOptionsAsync(connection, addressFamily, cancellationToken)
               .ConfigureAwait(false);

            if (port > 0 && pasvOptions.HasPortRange && (port > pasvOptions.PasvMaxPort || port < pasvOptions.PasvMinPort))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(port),
                    $"must be in {pasvOptions.PasvMinPort}:{pasvOptions.PasvMaxPort}");
            }

            if (port == 0 && pasvOptions.HasPortRange)
            {
                listener = CreateListenerInRange(connection, pasvOptions);
            }
            else
            {
                listener = new PasvListener(connection.LocalEndPoint.Address, port, pasvOptions.PublicAddress);
            }

            return listener;
        }

        /// <summary>
        /// Gets a listener on a port within the assigned port range.
        /// </summary>
        /// <param name="connection">Connection for which to create the listener.</param>
        /// <param name="pasvOptions">The options for the <see cref="IPasvListener"/>.</param>
        /// <returns>Configured PasvListener.</returns>
        /// <exception cref="SocketException">When no free port could be found, or other bad things happen. See <see cref="SocketError"/>.</exception>
        private IPasvListener CreateListenerInRange(IFtpConnection connection, PasvListenerOptions pasvOptions)
        {
            lock (_listenerLock)
            {
                // randomize ports so we don't always get the ports in the same order
                foreach (var port in GetPorts(pasvOptions))
                {
                    try
                    {
                        return new PasvListener(connection.LocalEndPoint.Address, port, pasvOptions.PublicAddress);
                    }
                    catch (SocketException se)
                    {
                        // retry if the socket is already in use, else throw the underlying exception
                        if (se.SocketErrorCode != SocketError.AddressAlreadyInUse)
                        {
                            _log?.LogError(se, "Could not create listener");
                            throw;
                        }
                    }
                }

                // if we reach this point, we have not been able to create a listener within range
                _log?.LogWarning("No free ports available for data connection");
                throw new SocketException((int)SocketError.AddressAlreadyInUse);
            }
        }

        private IEnumerable<int> GetPorts(PasvListenerOptions options)
        {
            var portRangeCount = (options.PasvMaxPort - options.PasvMinPort + 1) * 2;
            while (portRangeCount-- != 0)
            {
                yield return _prng.Next(options.PasvMinPort, options.PasvMaxPort + 1);
            }
        }
    }
}
