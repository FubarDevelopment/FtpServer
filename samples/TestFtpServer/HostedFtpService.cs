// <copyright file="HostedFtpService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;

using JetBrains.Annotations;

using Microsoft.Extensions.Hosting;

using TestFtpServer.FtpServerShell;

namespace TestFtpServer
{
    /// <summary>
    /// Generic host for the FTP server.
    /// </summary>
    public class HostedFtpService : IHostedService, IDisposable
    {
        [NotNull]
        private readonly IFtpServerHost _ftpServerHost;

        [NotNull]
        private readonly IDisposable _serverShutdown;

        public HostedFtpService(
            [NotNull] IFtpServerHost ftpServerHost,
            [NotNull] IApplicationLifetime applicationLifetime,
            [NotNull] IShellStatus shellStatus)
        {
            _serverShutdown = applicationLifetime.ApplicationStopping.Register(() =>
            {
                shellStatus.Closed = true;
            });
            _ftpServerHost = ftpServerHost;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _ftpServerHost.StartAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _ftpServerHost.StopAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _serverShutdown.Dispose();
        }
    }
}
