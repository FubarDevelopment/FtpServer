//-----------------------------------------------------------------------
// <copyright file="FtpServer.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;

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
        private readonly object _startedLock = new object();

        /// <summary>
        /// Mutext for Stopped field.
        /// </summary>
        private readonly object _stopLocker = new object();

        [NotNull]
        private readonly FtpServerStatistics _statistics = new FtpServerStatistics();

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentDictionary<IFtpConnection, FtpConnectionInfo> _connections = new ConcurrentDictionary<IFtpConnection, FtpConnectionInfo>();

        [CanBeNull]
        private readonly ILogger<FtpServer> _log;

        /// <summary>
        /// Don't use this directly, use the Stopped property instead. It is protected by a mutex.
        /// </summary>
        private bool _stopped;

        private ConfiguredTaskAwaitable _listenerTask;

        private volatile bool _isReady;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <param name="serverOptions">The server options.</param>
        /// <param name="serviceProvider">The service provider used to query services.</param>
        /// <param name="logger">The FTP server logger.</param>
        /// <param name="loggerFactory">Factory for loggers.</param>
        public FtpServer(
            [NotNull] IOptions<FtpServerOptions> serverOptions,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] ILogger<FtpServer> logger = null,
            [CanBeNull] ILoggerFactory loggerFactory = null)
        {
            _serviceProvider = serviceProvider;
            _log = logger;
            ServerAddress = serverOptions.Value.ServerAddress;
            Port = serverOptions.Value.Port;
            BackgroundTransferWorker = new BackgroundTransferWorker(loggerFactory?.CreateLogger<BackgroundTransferWorker>());
            BackgroundTransferWorker.Start(_cancellationTokenSource);
        }

        /// <inheritdoc />
        public event EventHandler<ConnectionEventArgs> ConfigureConnection;

        /// <inheritdoc />
        public IFtpServerStatistics Statistics => _statistics;

        /// <inheritdoc />
        public string ServerAddress { get; }

        /// <inheritdoc />
        public int Port { get; }

        /// <inheritdoc />
        public bool Ready
        {
            get
            {
                lock (_startedLock)
                {
                    return _isReady;
                }
            }

            private set
            {
                lock (_startedLock)
                {
                    _isReady = value;
                }
            }
        }

        private BackgroundTransferWorker BackgroundTransferWorker { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the server is stopped.
        /// </summary>
        /// <remarks>
        /// Mutexed so it can be accessed concurrently by different threads.
        /// </remarks>
        private bool Stopped
        {
            get
            {
                lock (_stopLocker)
                {
                    return _stopped;
                }
            }
            set
            {
                lock (_stopLocker)
                {
                    _stopped = value;
                }
            }
        }

        /// <inheritdoc />
        public void Start()
        {
            if (Stopped)
            {
                throw new InvalidOperationException("Cannot start a previously stopped FTP server");
            }

            _listenerTask = ExecuteServerListener().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Stop()
        {
            _cancellationTokenSource.Cancel(true);
            Stopped = true;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<BackgroundTransferInfo> GetBackgroundTaskStates()
        {
            return BackgroundTransferWorker.GetStates();
        }

        /// <inheritdoc />
        public void EnqueueBackgroundTransfer(IBackgroundTransfer backgroundTransfer, IFtpConnection connection)
        {
            var entry = new BackgroundTransferEntry(backgroundTransfer, connection?.Log);
            BackgroundTransferWorker.Enqueue(entry);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!Stopped)
            {
                Stop();
            }

            try
            {
                _listenerTask.GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                // Ignorieren - alles ist OK
            }

            BackgroundTransferWorker.Dispose();
            _cancellationTokenSource.Dispose();
            foreach (var connectionInfo in _connections.Values)
            {
                connectionInfo.Scope.Dispose();
            }
        }

        private void OnConfigureConnection(IFtpConnection connection)
        {
            ConfigureConnection?.Invoke(this, new ConnectionEventArgs(connection));
        }

        private Task ExecuteServerListener()
        {
            return Task.Run(async () =>
            {
                var listener = new MultiBindingTcpListener(ServerAddress, Port);
                try
                {
                    await listener.StartAsync().ConfigureAwait(false);

                    Ready = true;

                    try
                    {
                        while (!Stopped)
                        {
                            if (listener.TryGetPending(out var tcpListener))
                            {
                                var acceptTask = tcpListener.AcceptTcpClientAsync();
                                acceptTask.Wait(_cancellationTokenSource.Token);
                                var client = acceptTask.Result;
                                AddClient(client);
                                continue;
                            }

                            Thread.Sleep(0);
                        }
                    }
                    finally
                    {
                        listener.Stop();

                        foreach (var connection in _connections.Keys.ToList())
                        {
                            connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log?.LogCritical(ex, "{0}", ex.Message);
                }
            });
        }

        private void AddClient(TcpClient client)
        {
            try
            {
                var scope = _serviceProvider.CreateScope();
                var socketAccessor = scope.ServiceProvider.GetRequiredService<TcpSocketClientAccessor>();
                socketAccessor.TcpSocketClient = client;

                var connection = scope.ServiceProvider.GetRequiredService<IFtpConnection>();
                var connectionAccessor = scope.ServiceProvider.GetRequiredService<IFtpConnectionAccessor>();
                connectionAccessor.FtpConnection = connection;

                if (!_connections.TryAdd(connection, new FtpConnectionInfo(scope)))
                {
                    scope.Dispose();
                    return;
                }

                _statistics.ActiveConnections += 1;
                _statistics.TotalConnections += 1;
                connection.Closed += ConnectionOnClosed;
                OnConfigureConnection(connection);
                connection.Start();
            }
            catch (Exception ex)
            {
                _log?.LogError(ex, ex.Message);
            }
        }

        private void ConnectionOnClosed(object sender, EventArgs eventArgs)
        {
            if (Stopped)
            {
                return;
            }

            var connection = (IFtpConnection)sender;
            if (!_connections.TryRemove(connection, out var info))
            {
                return;
            }

            info.Scope.Dispose();
            _statistics.ActiveConnections -= 1;
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
