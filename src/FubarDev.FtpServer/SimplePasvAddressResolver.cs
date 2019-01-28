// <copyright file="SimplePasvAddressResolver.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The default implementation of the <see cref="SimplePasvAddressResolver"/>.
    /// </summary>
    public class SimplePasvAddressResolver : IPasvAddressResolver
    {
        private readonly SimplePasvOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePasvAddressResolver"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SimplePasvAddressResolver(IOptions<SimplePasvOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public Task<PasvListenerOptions> GetOptionsAsync(IFtpConnection connection, CancellationToken cancellationToken)
        {
            var minPort = _options.PasvMinPort ?? 0;
            if (minPort > 0 && minPort < 1024)
            {
                minPort = 1024;
            }

            var maxPort = Math.Max(_options.PasvMaxPort ?? 0, minPort);

            var publicAddress = _options.PublicAddress ?? connection.LocalEndPoint.Address;

            return Task.FromResult(new PasvListenerOptions(minPort, maxPort, publicAddress));
        }
    }
}
