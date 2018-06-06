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
    public sealed class FtpServer : IDisposable
    {
        private readonly object _startedLock = new object();

        /// <summary>
        /// Mutext for Stopped field.
        /// </summary>
        private readonly object _stopLocker = new object();

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

        /// <summary>
        /// This event is raised when the connection is ready to be configured.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConfigureConnection;

        /// <summary>
        /// Gets the FTP server statistics.
        /// </summary>
        [NotNull]
        public FtpServerStatistics Statistics { get; } = new FtpServerStatistics();

        /// <summary>
        /// Gets the public IP address (required for <code>PASV</code> and <code>EPSV</code>).
        /// </summary>
        [NotNull]
        public string ServerAddress { get; }

        /// <summary>
        /// Gets the port on which the FTP server is listening for incoming connections.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets or sets a value indicating whether server ready to receive incoming connectoions.
        /// </summary>
        public bool Ready
        {
            get
            {
                lock (_startedLock)
                {
                    return _isReady;
                }
            }

            set
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

        /// <summary>
        /// Starts the FTP server in the background.
        /// </summary>
        public void Start()
        {
            if (Stopped)
            {
                throw new InvalidOperationException("Cannot start a previously stopped FTP server");
            }

            _listenerTask = ExecuteServerListener().ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the FTP server.
        /// </summary>
        /// <remarks>
        /// The FTP server cannot be started again after it was stopped.
        /// </remarks>
        public void Stop()
        {
            _cancellationTokenSource.Cancel(true);
            Stopped = true;
        }

        /// <summary>
        /// Get the background transfer states for all active <see cref="IBackgroundTransfer"/> objects.
        /// </summary>
        /// <returns>The background transfer states for all active <see cref="IBackgroundTransfer"/> objects.</returns>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<BackgroundTransferInfo> GetBackgroundTaskStates()
        {
            return BackgroundTransferWorker.GetStates();
        }

        /// <summary>
        /// Enqueue a new <see cref="IBackgroundTransfer"/> for the given <paramref name="connection"/>.
        /// </summary>
        /// <param name="backgroundTransfer">The background transfer to enqueue.</param>
        /// <param name="connection">The connection to enqueue the background transfer for.</param>
        public void EnqueueBackgroundTransfer([NotNull] IBackgroundTransfer backgroundTransfer, [CanBeNull] IFtpConnection connection)
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

                Statistics.ActiveConnections += 1;
                Statistics.TotalConnections += 1;
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
            Statistics.ActiveConnections -= 1;
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
