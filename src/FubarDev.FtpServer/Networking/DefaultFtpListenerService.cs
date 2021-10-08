// <copyright file="DefaultFtpListenerService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.Networking
{
    /// <summary>
    /// Default implementation of <see cref="IFtpListenerService"/>.
    /// </summary>
    internal class DefaultFtpListenerService : IFtpListenerService, IDisposable
    {
        private readonly CancellationTokenSource _serverShutdown = new CancellationTokenSource();
        private readonly Channel<TcpClient> _channels = System.Threading.Channels.Channel.CreateBounded<TcpClient>(5);
        private readonly FtpServerListenerService _listenerService;

        public DefaultFtpListenerService(
            IOptions<FtpServerOptions> serverOptions,
            ILogger<DefaultFtpListenerService>? logger = null)
        {
            _listenerService = new FtpServerListenerService(
                _channels.Writer,
                serverOptions,
                _serverShutdown,
                logger);
            _listenerService.ListenerStarted += (sender, args) => ListenerStarted?.Invoke(sender, args);
        }

        /// <inheritdoc />
        public event EventHandler<ListenerStartedEventArgs>? ListenerStarted;

        /// <inheritdoc />
        public CancellationTokenSource ListenerShutdown => _serverShutdown;

        /// <inheritdoc />
        public ChannelReader<TcpClient> Channel => _channels.Reader;

        /// <inheritdoc />
        public FtpServiceStatus Status => _listenerService.Status;

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
            => _listenerService.StartAsync(cancellationToken);

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
            => _listenerService.StopAsync(cancellationToken);

        /// <inheritdoc />
        public Task PauseAsync(CancellationToken cancellationToken)
            => _listenerService.PauseAsync(cancellationToken);

        /// <inheritdoc />
        public Task ContinueAsync(CancellationToken cancellationToken)
            => _listenerService.ContinueAsync(cancellationToken);

        /// <inheritdoc />
        public void Dispose()
        {
            _serverShutdown.Dispose();
        }
    }
}
