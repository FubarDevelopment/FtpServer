//-----------------------------------------------------------------------
// <copyright file="FtpServer.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using FubarDev.FtpServer.ConnectionChecks;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Localization;
using FubarDev.FtpServer.Networking;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Statistics;

using Microsoft.AspNetCore.Connections.Features;
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
        private readonly FtpServerStatistics _statistics = new FtpServerStatistics();
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IFtpControlStreamAdapter> _controlStreamAdapters;
        private readonly ConcurrentDictionary<IFtpConnection, FtpConnectionInfo> _connections = new ConcurrentDictionary<IFtpConnection, FtpConnectionInfo>();
        private readonly FtpServerListenerService _serverListener;
        private readonly ILogger<FtpServer>? _log;
        private readonly Task _clientReader;
        private readonly Task _connectionStopper;
        private readonly CancellationTokenSource _serverShutdown = new CancellationTokenSource();
        private readonly Timer? _connectionTimeoutChecker;
        private readonly Channel<IFtpConnection> _stoppedConnectionChannel = Channel.CreateUnbounded<IFtpConnection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="serverOptions">The server options.</param>
        /// <param name="serviceProvider">The service provider used to query services.</param>
        /// <param name="controlStreamAdapters">Adapters for the control connection stream.</param>
        /// <param name="logger">The FTP server logger.</param>
        public FtpServer(
            IOptions<FtpServerOptions> serverOptions,
            IServiceProvider serviceProvider,
            IEnumerable<IFtpControlStreamAdapter> controlStreamAdapters,
            ILogger<FtpServer>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _controlStreamAdapters = controlStreamAdapters.ToList();
            _log = logger;
            ServerAddress = serverOptions.Value.ServerAddress;
            Port = serverOptions.Value.Port;
            MaxActiveConnections = serverOptions.Value.MaxActiveConnections;

            var tcpClientChannel = Channel.CreateBounded<TcpClient>(5);
            _serverListener = new FtpServerListenerService(tcpClientChannel, serverOptions, _serverShutdown, logger);
            _serverListener.ListenerStarted += (s, e) =>
            {
                Port = e.Port;
                OnListenerStarted(e);
            };

            _clientReader = ReadClientsAsync(tcpClientChannel, _serverShutdown.Token);
            _connectionStopper = StopConnectionsAsync(_stoppedConnectionChannel.Reader, _serverShutdown.Token);

            if (serverOptions.Value.ConnectionInactivityCheckInterval is TimeSpan checkInterval)
            {
                _connectionTimeoutChecker = new Timer(
                    _ => CloseExpiredConnections(),
                    null,
                    checkInterval,
                    checkInterval);
            }
        }

        /// <inheritdoc />
        public event EventHandler<ConnectionEventArgs>? ConfigureConnection;

        /// <inheritdoc />
        public event EventHandler<ListenerStartedEventArgs>? ListenerStarted;

        /// <inheritdoc />
        public IFtpServerStatistics Statistics => _statistics;

        /// <inheritdoc />
        public string? ServerAddress { get; }

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

            _connectionTimeoutChecker?.Dispose();

            _serverShutdown.Dispose();
        }

        /// <summary>
        /// Returns all connections.
        /// </summary>
        /// <returns>The currently active connections.</returns>
        /// <remarks>
        /// The connection might be closed between calling this function and
        /// using/querying the connection by the client.
        /// </remarks>
        public IEnumerable<IFtpConnection> GetConnections()
        {
            return _connections.Keys.ToList();
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
            if (!_serverShutdown.IsCancellationRequested)
            {
                _serverShutdown.Cancel(true);
            }

            await _serverListener.StopAsync(cancellationToken).ConfigureAwait(false);
            await _clientReader.ConfigureAwait(false);
            await _connectionStopper.ConfigureAwait(false);
        }

        /// <summary>
        /// Close expired FTP connections.
        /// </summary>
        /// <remarks>
        /// This will always happen when the FTP client is idle (without sending notifications) or
        /// when the client was disconnected due to an undetectable network error.
        /// </remarks>
        private void CloseExpiredConnections()
        {
            foreach (var connection in GetConnections())
            {
                try
                {
                    var keepAliveFeature = connection.Features.Get<IFtpConnectionStatusCheck>();
                    var isAlive = keepAliveFeature.CheckIfAlive();
                    if (isAlive)
                    {
                        // Ignore connections that are still alive.
                        continue;
                    }

                    var serverCommandFeature = connection.Features.Get<IServerCommandFeature>();

                    // Just ignore a failed write operation. We'll try again later.
                    serverCommandFeature.ServerCommandWriter.TryWrite(
                        new CloseConnectionServerCommand());
                }
                catch
                {
                    // Errors are most likely indicating a closed connection. Nothing to do here...
                }
            }
        }

        private IEnumerable<ConnectionInitAsyncDelegate> OnConfigureConnection(IFtpConnection connection)
        {
            var eventArgs = new ConnectionEventArgs(connection);
            ConfigureConnection?.Invoke(this, eventArgs);
            return eventArgs.AsyncInitFunctions;
        }

        private async Task ReadClientsAsync(
            ChannelReader<TcpClient> tcpClientReader,
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
                while (exception is AggregateException aggregateException && aggregateException.InnerException != null)
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
                _log?.LogDebug("Stopped accepting connections");
            }
        }

        private async Task StopConnectionsAsync(
            ChannelReader<IFtpConnection> stoppedFtpClients,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var hasConnection = await stoppedFtpClients.WaitToReadAsync(cancellationToken)
                       .ConfigureAwait(false);
                    if (!hasConnection)
                    {
                        return;
                    }

                    while (stoppedFtpClients.TryRead(out var connection))
                    {
                        await StopConnectionAsync(connection)
                           .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex) when (ex.Is<OperationCanceledException>())
            {
                // Ignore
            }
            catch (Exception ex)
            {
                _log?.LogWarning(ex, ex.Message);
            }
            finally
            {
                _log?.LogDebug("Stopped accepting connections");
            }

            // Stop all remaining connections
            foreach (var connection in _connections.Keys.ToList())
            {
                await StopConnectionAsync(connection)
                   .ConfigureAwait(false);
            }
        }

        private async Task AddClientAsync(TcpClient client)
        {
            var scope = _serviceProvider.CreateScope();
            try
            {
                Stream socketStream = client.GetStream();
                foreach (var controlStreamAdapter in _controlStreamAdapters)
                {
                    socketStream = await controlStreamAdapter.WrapAsync(socketStream, CancellationToken.None)
                       .ConfigureAwait(false);
                }

                // Initialize information about the socket
                var socketAccessor = scope.ServiceProvider.GetRequiredService<TcpSocketClientAccessor>();
                socketAccessor.TcpSocketClient = client;
                socketAccessor.TcpSocketStream = socketStream;

                // Create the connection
                var connection = scope.ServiceProvider.GetRequiredService<IFtpConnection>();
                var connectionAccessor = scope.ServiceProvider.GetRequiredService<IFtpConnectionAccessor>();
                connectionAccessor.FtpConnection = connection;

                // Get access to the connection lifetime
                var lifetimeFeature = connection.Features.Get<IConnectionLifetimeFeature>();

                // Registration to get notified when the connection should be stopped.
                var stopRegistration = lifetimeFeature.ConnectionClosed.Register(() => RegisterForStop(connection));

                // Register statistics collectors
                var statisticsCollectors =
                    scope.ServiceProvider.GetRequiredService<IEnumerable<IFtpStatisticsCollector>>();
                var statCollFeature = connection.Features.Get<IFtpStatisticsCollectorFeature>();
                var statCollRegistrations = statisticsCollectors
                   .Select(statCollFeature.Register)
                   .ToList();

                // Create connection information
                var connectionInfo = new FtpConnectionInfo(scope, stopRegistration, statCollRegistrations);

                // Remember connection
                if (!_connections.TryAdd(connection, connectionInfo))
                {
                    _log.LogCritical("A new scope was created, but the connection couldn't be added to the list.");
                    client.Dispose();
                    scope.Dispose();
                    return;
                }

                // Send initial message
                var serverCommandWriter = connection.Features.Get<IServerCommandFeature>().ServerCommandWriter;

                var blockConnection = MaxActiveConnections != 0
                    && _statistics.ActiveConnections >= MaxActiveConnections;
                if (blockConnection)
                {
                    // Send response
                    var response = new FtpResponse(421, "Too many users, server is full.");
                    await serverCommandWriter.WriteAsync(new SendResponseServerCommand(response))
                       .ConfigureAwait(false);

                    // Send close
                    await serverCommandWriter.WriteAsync(new CloseConnectionServerCommand())
                       .ConfigureAwait(false);
                }
                else
                {
                    var serverMessages = scope.ServiceProvider.GetRequiredService<IFtpServerMessages>();
                    var response = new FtpResponseTextBlock(220, serverMessages.GetBannerMessage());

                    // Send initial response
                    await serverCommandWriter.WriteAsync(
                            new SendResponseServerCommand(response),
                            lifetimeFeature.ConnectionClosed)
                       .ConfigureAwait(false);
                }

                // Statistics
                _statistics.AddConnection();

                // Connection configuration by host
                var asyncInitFunctions = OnConfigureConnection(connection);
                foreach (var asyncInitFunction in asyncInitFunctions)
                {
                    await asyncInitFunction(connection, CancellationToken.None)
                       .ConfigureAwait(false);
                }

                // Start connection
                await connection.StartAsync()
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                scope.Dispose();
                _log?.LogError(ex, ex.Message);
            }
        }

        private async void RegisterForStop(IFtpConnection connection)
        {
            await _stoppedConnectionChannel.Writer.WriteAsync(connection)
               .ConfigureAwait(false);
        }

        private async Task StopConnectionAsync(IFtpConnection connection)
        {
            if (!_connections.TryRemove(connection, out var info))
            {
                return;
            }

            await connection.StopAsync().ConfigureAwait(false);

            foreach (var registration in info.StatisticsRegistrations)
            {
                registration.Dispose();
            }

            info.Registration.Dispose();
            info.Scope.Dispose();

            _statistics.CloseConnection();
        }

        private void OnListenerStarted(ListenerStartedEventArgs e)
        {
            ListenerStarted?.Invoke(this, e);
        }

        private class FtpConnectionInfo
        {
            public FtpConnectionInfo(
                IServiceScope scope,
                CancellationTokenRegistration registration,
                IReadOnlyCollection<IDisposable> statisticsRegistrations)
            {
                Scope = scope;
                Registration = registration;
                StatisticsRegistrations = statisticsRegistrations;
            }

            public IServiceScope Scope { get; }
            public CancellationTokenRegistration Registration { get; }
            public IReadOnlyCollection<IDisposable> StatisticsRegistrations { get; }
        }
    }
}
