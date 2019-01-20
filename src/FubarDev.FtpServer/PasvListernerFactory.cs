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
    /// Handles a pool of ports for use with PASV
    /// </summary>
    public class PasvListernerFactory : IPasvListernerFactory
    {
        private readonly ILogger<PasvListernerFactory> _log;

        private const int ErrorEndpointInUse = 10048;

        private readonly int _pasvMinPort;
        private readonly int _pasvMaxPort;

        private readonly int[] _pasvPorts;

        private readonly Random _prng = new Random();

        public PasvListernerFactory(IOptions<FtpServerOptions> serverOptions, ILogger<PasvListernerFactory> logger)
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

        /// <inheritdoc />
        public Task<IPasvListener> CreateTcpLister(IFtpConnection connection)
        {
            return CreateTcpLister(connection, 0);
        }

        /// <inheritdoc />
        public Task<IPasvListener> CreateTcpLister(IFtpConnection connection, int port)
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
        /// Gets a listener on a port within the assigned port range
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="SocketException"></exception>
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
                            throw;
                        }
                    }
                }

                // if we reach this point, we have not been able to create a listener within range
                throw new SocketException((int)SocketError.AddressAlreadyInUse);
            }
        }

        protected IPAddress PasvExternalAddress { get; }
    }
}
