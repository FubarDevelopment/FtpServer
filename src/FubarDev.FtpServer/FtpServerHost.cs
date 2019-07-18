// <copyright file="FtpServerHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Simple <see cref="IFtpServerHost"/> implementation.
    /// </summary>
    /// <remarks>
    /// This services is used to start and stop all <see cref="IFtpService"/> instances.
    /// </remarks>
    public class FtpServerHost : IFtpServerHost
    {
        private readonly IReadOnlyCollection<IFtpService> _ftpServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServerHost"/> class.
        /// </summary>
        /// <param name="ftpServices">The FTP services to start and stop.</param>
        public FtpServerHost(IEnumerable<IFtpService> ftpServices)
        {
            _ftpServices = ftpServices.ToList();
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var ftpService in _ftpServices)
            {
                await ftpService.StartAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var ftpService in _ftpServices.Reverse())
            {
                await ftpService.StopAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
