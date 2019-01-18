// <copyright file="PasvPortHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Handles a pool of ports for use with PASV
    /// </summary>
    public class PasvPortPool : IPasvPortPool, IDisposable
    {
        private readonly ILogger<PasvPortPool> _log;

        private readonly Dictionary<int, bool> _pasvPorts;

        private readonly SemaphoreSlim _mutex;

        private int _pasvPortsAvailable;

        private readonly Random _prng = new Random();

        public PasvPortPool(IOptions<FtpServerOptions> serverOptions, ILogger<PasvPortPool> logger)
        {
            _log = logger;
            if (serverOptions.Value.PasvMinPort > 1023 && serverOptions.Value.PasvMaxPort >= serverOptions.Value.PasvMinPort)
            {
                _log.LogInformation($"PASV port range set to {serverOptions.Value.PasvMinPort}:{serverOptions.Value.PasvMaxPort}");
                _pasvPorts = new Dictionary<int, bool>();

                for (var i = serverOptions.Value.PasvMinPort; i <= serverOptions.Value.PasvMaxPort; i++)
                {
                    _pasvPorts[i] = true;
                }

                _pasvPortsAvailable = _pasvPorts.Count;

                _mutex = new SemaphoreSlim(1, 1);
            }
        }

        /// <inheritdoc />
        public Task<int> LeasePasvPort()
        {
            return LeasePasvPort(0);
        }

        /// <summary>
        /// Return a random free passive port.
        /// </summary>
        /// <param name="desiredPort">If set to != 0, get this specific port, not the next free one</param>
        /// <returns>A free port, or 0 if any port can be chosen, or -1 if there is no free port</returns>
        public async Task<int> LeasePasvPort(int desiredPort)
        {
            if (_pasvPorts == null)
            {
                return desiredPort;
            }

            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                int port;
                if (desiredPort == 0)
                {
                    if (_pasvPortsAvailable == 0)
                    {
                        return -1;
                    }

                    // choose a random port, and set _pasvPortsAvailable to false if you got the last one
                    var availablePorts = _pasvPorts.Where(_ => _.Value).Select(_ => _.Key).ToArray();
                    var index = _prng.Next(0, availablePorts.Length - 1);
                    port = availablePorts[index];
                }
                else
                {
                    // we want a specific port

                    if (!_pasvPorts.ContainsKey(desiredPort))
                    {
                        _log.LogWarning($"Requested PASV port number {desiredPort} outside of available range");
                        return -1;
                    }

                    if (!_pasvPorts[desiredPort])
                    {
                        return -1;
                    }

                    port = desiredPort;
                }

                _pasvPorts[port] = false;
                _pasvPortsAvailable--;
                return port;

            }
            finally
            {
                _mutex.Release();
            }

        }

        /// <summary>
        /// Return the port to the pool of available ports.
        /// </summary>
        /// <param name="port"></param>
        public async Task ReturnPasvPort(int port)
        {
            if (_pasvPorts != null)
            {
                await _mutex.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (!_pasvPorts.ContainsKey(port))
                    {
                        throw new ArgumentException("Port must be a previously leased port", nameof(port));
                    }

                    _pasvPorts[port] = true;
                    _pasvPortsAvailable++;
                }
                finally
                {
                    _mutex.Release();
                }
            }
        }


        /// <inheritdoc />
        public void Dispose()
        {
            _mutex.Dispose();
        }
    }
}
