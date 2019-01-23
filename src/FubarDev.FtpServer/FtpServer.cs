//-----------------------------------------------------------------------
// <copyright file="FtpServer.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The portable FTP server.
    /// </summary>
    public sealed class FtpServer : IFtpServer, IFtpService, IDisposable
    {
        /// <summary>
        /// Mutex for Ready field.
        /// </summary>
        private readonly object _startedLock = new object();

        /// <summary>
        /// Mutex for Stopped field.
        /// </summary>
        private readonly object _stopLocker = new object();

        /// <summary>
        /// Semaphore that gets released when the listener stopped.
        /// </summary>
        private readonly SemaphoreSlim _stoppedSemaphore = new SemaphoreSlim(0, 1);

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
        private volatile bool _stopped;

        private volatile bool _isReady;

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

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!Stopped)
            {
                ((IFtpService)this).StopAsync(CancellationToken.None).Wait();
            }

            _cancellationTokenSource.Dispose();
            foreach (var connectionInfo in _connections.Values)
            {
                connectionInfo.Scope.Dispose();
            }

            _stoppedSemaphore.Dispose();
        }

        /// <inheritdoc />
        void IFtpServer.Start()
        {
            var host = _serviceProvider.GetRequiredService<IFtpServerHost>();
            host.StartAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        void IFtpServer.Stop()
        {
            var host = _serviceProvider.GetRequiredService<IFtpServerHost>();
            host.StopAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        async Task IFtpService.StartAsync(CancellationToken cancellationToken)
        {
            if (Stopped)
            {
                throw new InvalidOperationException("Cannot start a previously stopped FTP server");
            }

            using (var semaphore = new SemaphoreSlim(0, 1))
            {
                ExecuteServerListener(semaphore);
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                semaphore.Release();
            }
        }

        /// <inheritdoc />
        Task IFtpService.StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel(true);
            Stopped = true;
            return _stoppedSemaphore.WaitAsync(cancellationToken);
        }

        private void OnConfigureConnection(IFtpConnection connection)
        {
            ConfigureConnection?.Invoke(this, new ConnectionEventArgs(connection));
        }

        private void ExecuteServerListener(SemaphoreSlim semaphore)
        {
            Task.Run(async () =>
            {
                var listener = new MultiBindingTcpListener(ServerAddress, Port, _log);
                try
                {
                    await listener.StartAsync().ConfigureAwait(false);
                    listener.StartAccepting();

                    Ready = true;
                    semaphore.Release();

                    try
                    {
                        while (!Stopped && !_cancellationTokenSource.IsCancellationRequested)
                        {
                            var acceptTask = listener.WaitAnyTcpClientAsync(_cancellationTokenSource.Token);
                            var client = acceptTask.Result;
                            AddClient(client);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore - everything is fine
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
                finally
                {
                    _stoppedSemaphore.Release();
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
