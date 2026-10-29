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
using FubarDev.FtpServer.ServerCommands;

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
        private readonly List<IFtpConnectionConfigurator> _connectionConfigurators;
        private readonly List<IFtpControlStreamAdapter> _controlStreamAdapters;
        private readonly ConcurrentDictionary<IFtpConnection, FtpConnectionInfo> _connections = new ConcurrentDictionary<IFtpConnection, FtpConnectionInfo>();
        private readonly IFtpListenerService _serverListener;
        private readonly ILogger<FtpServer>? _log;
        private readonly Task _clientReader;
        private readonly Timer? _connectionTimeoutChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="serverOptions">The server options.</param>
        /// <param name="serviceProvider">The service provider used to query services.</param>
        /// <param name="controlStreamAdapters">Adapters for the control connection stream.</param>
        /// <param name="connectionConfigurators">Configurators for FTP connections.</param>
        /// <param name="ftpListenerService">Listener service for FTP connections.</param>
        /// <param name="logger">The FTP server logger.</param>
        public FtpServer(
            IOptions<FtpServerOptions> serverOptions,
            IServiceProvider serviceProvider,
            IEnumerable<IFtpControlStreamAdapter> controlStreamAdapters,
            IEnumerable<IFtpConnectionConfigurator> connectionConfigurators,
            IFtpListenerService ftpListenerService,
            ILogger<FtpServer>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _connectionConfigurators = connectionConfigurators.ToList();
            _controlStreamAdapters = controlStreamAdapters.ToList();
            _log = logger;
            ServerAddress = serverOptions.Value.ServerAddress;
            Port = serverOptions.Value.Port;
            MaxActiveConnections = serverOptions.Value.MaxActiveConnections;

            _serverListener = ftpListenerService;
            _serverListener.ListenerStarted += (s, e) =>
            {
                Port = e.Port;
                OnListenerStarted(e);
            };

            _clientReader = ReadClientsAsync(
                _serverListener.Channel,
                _serverListener.ListenerShutdown.Token);

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

            (_serverListener as IDisposable)?.Dispose();
            foreach (var connectionInfo in _connections.Values)
            {
                connectionInfo.Scope.Dispose();
            }
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
        [Obsolete("Use IFtpServerHost.StartAsync instead.")]
        void IFtpServer.Start()
        {
            var host = _serviceProvider.GetRequiredService<IFtpServerHost>();
            host.StartAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        [Obsolete("Use IFtpServerHost.StopAsync instead.")]
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
            if (!_serverListener.ListenerShutdown.IsCancellationRequested)
            {
                _serverListener.ListenerShutdown.Cancel(true);
            }

            await _serverListener.StopAsync(cancellationToken).ConfigureAwait(false);
            await _clientReader.ConfigureAwait(false);
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

                // Remember connection
                if (!_connections.TryAdd(connection, new FtpConnectionInfo(scope)))
                {
                    _log?.LogCritical("A new scope was created, but the connection couldn't be added to the list");
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
                            connection.CancellationToken)
                       .ConfigureAwait(false);
                }

                // Statistics
                _statistics.AddConnection();

                // Statistics and cleanup
                connection.Closed += ConnectionOnClosed;

                // Connection configuration by host
                var asyncInitFunctions = OnConfigureConnection(connection);
                foreach (var asyncInitFunction in asyncInitFunctions)
                {
                    await asyncInitFunction(connection, CancellationToken.None)
                       .ConfigureAwait(false);
                }

                // Configure the connection
                foreach (var connectionConfigurator in _connectionConfigurators)
                {
                    await connectionConfigurator.Configure(connection, CancellationToken.None)
                       .ConfigureAwait(false);
                }

                // Start connection
                await connection.StartAsync()
                   .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                scope.Dispose();
                _log?.LogError(ex, "Failed to start the client connection: {ErrorMessage}", ex.Message);
            }
        }

        private void ConnectionOnClosed(object? sender, EventArgs eventArgs)
        {
            var connection = (IFtpConnection)(sender ?? throw new InvalidOperationException("Missing sender information."));
            if (!_connections.TryRemove(connection, out var info))
            {
                return;
            }

            info.Scope.Dispose();

            _statistics.CloseConnection();
        }

        private void OnListenerStarted(ListenerStartedEventArgs e)
        {
            ListenerStarted?.Invoke(this, e);
        }

        private class FtpConnectionInfo
        {
            public FtpConnectionInfo(IServiceScope scope)
            {
                Scope = scope;
            }

            public IServiceScope Scope { get; }
        }
    }
}
