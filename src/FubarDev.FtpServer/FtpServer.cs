//-----------------------------------------------------------------------
// <copyright file="FtpServer.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Networking;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The portable FTP server.
    /// </summary>
    public sealed class FtpServer : IFtpServer, IDisposable
    {
        [NotNull]
        private readonly FtpServerStatistics _statistics = new FtpServerStatistics();

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentDictionary<IFtpConnection, FtpConnectionInfo> _connections = new ConcurrentDictionary<IFtpConnection, FtpConnectionInfo>();

        private readonly FtpServerListenerService _serverListener;

        [CanBeNull]
        private readonly ILogger<FtpServer> _log;

        [NotNull]
        private readonly Task _clientReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="serverOptions">The server options.</param>
        /// <param name="serviceProvider">The service provider used to query services.</param>
        /// <param name="logger">The FTP server logger.</param>
        public FtpServer(
            [NotNull] IOptions<FtpServerOptions> serverOptions,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] ILogger<FtpServer> logger = null)
        {
            _serviceProvider = serviceProvider;
            _log = logger;
            ServerAddress = serverOptions.Value.ServerAddress;
            Port = serverOptions.Value.Port;
            MaxActiveConnections = serverOptions.Value.MaxActiveConnections;

            var tcpClientChannel = Channel.CreateBounded<TcpClient>(5);
            _serverListener = new FtpServerListenerService(tcpClientChannel, serverOptions, _cancellationTokenSource, logger);
            _serverListener.ListenerStarted += (s, e) =>
            {
                Port = e.Port;
                OnListenerStarted(e);
            };

            _clientReader = ReadClientsAsync(tcpClientChannel, _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public event EventHandler<ConnectionEventArgs> ConfigureConnection;

        /// <inheritdoc />
        public event EventHandler<ListenerStartedEventArgs> ListenerStarted;

        /// <inheritdoc />
        public IFtpServerStatistics Statistics => _statistics;

        /// <inheritdoc />
        public string ServerAddress { get; }

        /// <inheritdoc />
        public int Port { get; private set; }

        /// <inheritdoc />
        public int MaxActiveConnections { get; }

        /// <inheritdoc />
        public FtpServiceStatus Status => _serverListener.Status;

        /// <inheritdoc />
        public bool Ready => Status == FtpServiceStatus.Running;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Status != FtpServiceStatus.Stopped)
            {
                StopAsync(CancellationToken.None).Wait();
            }

            _cancellationTokenSource.Dispose();
            foreach (var connectionInfo in _connections.Values)
            {
                connectionInfo.Scope.Dispose();
            }
        }

        /// <inheritdoc />
        [Obsolete("User IFtpServerHost.StartAsync instead.")]
        void IFtpServer.Start()
        {
            var host = _serviceProvider.GetRequiredService<IFtpServerHost>();
            host.StartAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        [Obsolete("User IFtpServerHost.StopAsync instead.")]
        void IFtpServer.Stop()
        {
            var host = _serviceProvider.GetRequiredService<IFtpServerHost>();
            host.StopAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task PauseAsync(CancellationToken cancellationToken)
        {
            return _serverListener.PauseAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task ContinueAsync(CancellationToken cancellationToken)
        {
            return _serverListener.ContinueAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serverListener.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel(true);
            await _serverListener.StopAsync(cancellationToken).ConfigureAwait(false);
            await _clientReader.ConfigureAwait(false);
        }

        private void OnConfigureConnection(IFtpConnection connection)
        {
            ConfigureConnection?.Invoke(this, new ConnectionEventArgs(connection));
        }

        private async Task ReadClientsAsync(
            [NotNull] ChannelReader<TcpClient> tcpClientReader,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var hasClient = await tcpClientReader.WaitToReadAsync(cancellationToken)
                       .ConfigureAwait(false);
                    if (!hasClient)
                    {
                        return;
                    }

                    while (tcpClientReader.TryRead(out var client))
                    {
                        await AddClientAsync(client)
                           .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = ex;
                while (exception is AggregateException aggregateException)
                {
                    exception = aggregateException.InnerException;
                }

                switch (exception)
                {
                    case OperationCanceledException _:
                        // Cancelled
                        break;
                    default:
                        throw;
                }
            }
            finally
            {
                _log?.LogDebug("Stopped accepting connections.");
            }
        }

        private async Task AddClientAsync(TcpClient client)
        {
            var scope = _serviceProvider.CreateScope();
            try
            {
                var socketAccessor = scope.ServiceProvider.GetRequiredService<TcpSocketClientAccessor>();
                socketAccessor.TcpSocketClient = client;

                var connection = scope.ServiceProvider.GetRequiredService<IFtpConnection>();
                var connectionAccessor = scope.ServiceProvider.GetRequiredService<IFtpConnectionAccessor>();
                connectionAccessor.FtpConnection = connection;

                if (MaxActiveConnections != 0 && _statistics.ActiveConnections >= MaxActiveConnections)
                {
                    var response = new FtpResponse(10068, "Too many users, server is full.");
                    var responseBuffer = Encoding.UTF8.GetBytes($"{response}\r\n");
                    var secureConnectionFeature = connection.Features.Get<ISecureConnectionFeature>();
                    secureConnectionFeature.OriginalStream.Write(responseBuffer, 0, responseBuffer.Length);
                    client.Dispose();
                    scope.Dispose();
                    return;
                }

                if (!_connections.TryAdd(connection, new FtpConnectionInfo(scope)))
                {
                    scope.Dispose();
                    return;
                }

                _statistics.AddConnection();
                connection.Closed += ConnectionOnClosed;
                OnConfigureConnection(connection);
                await connection.StartAsync()
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                scope.Dispose();
                _log?.LogError(ex, ex.Message);
            }
        }

        private void ConnectionOnClosed(object sender, EventArgs eventArgs)
        {
            if (Status == FtpServiceStatus.Stopped)
            {
                return;
            }

            var connection = (IFtpConnection)sender;
            if (!_connections.TryRemove(connection, out var info))
            {
                return;
            }

            info.Scope.Dispose();

            _statistics.CloseConnection();
        }

        private class FtpConnectionInfo
        {
            public FtpConnectionInfo(IServiceScope scope)
            {
                Scope = scope;
            }

            public IServiceScope Scope { get; }
        }

        private void OnListenerStarted(ListenerStartedEventArgs e)
        {
            ListenerStarted?.Invoke(this, e);
        }
    }
}
