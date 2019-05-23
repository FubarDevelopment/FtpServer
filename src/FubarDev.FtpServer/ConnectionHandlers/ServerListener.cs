// <copyright file="ServerListener.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    /// <summary>
    /// Listener for the server.
    /// </summary>
    /// <remarks>
    /// Accepting connections can be paused.
    /// </remarks>
    public class ServerListener : ICommunicationService
    {
        [NotNull]
        private readonly ChannelWriter<TcpClient> _newClientWriter;

        [CanBeNull]
        private readonly ILogger _logger;

        [NotNull]
        private readonly MultiBindingTcpListener _multiBindingTcpListener;

        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        private readonly CancellationTokenSource _connectionClosedCts;

        [NotNull]
        private CancellationTokenSource _jobPaused = new CancellationTokenSource();

        [NotNull]
        private Task _task = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerListener"/> class.
        /// </summary>
        /// <param name="newClientWriter">Channel that receives all accepted clients.</param>
        /// <param name="serverOptions">The server options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="connectionClosedCts">Cancellation token source for a closed connection.</param>
        public ServerListener(
            [NotNull] ChannelWriter<TcpClient> newClientWriter,
            [NotNull] IOptions<FtpServerOptions> serverOptions,
            CancellationTokenSource connectionClosedCts,
            [CanBeNull] ILogger logger = null)
        {
            _newClientWriter = newClientWriter;
            _logger = logger;
            _connectionClosedCts = connectionClosedCts;
            var options = serverOptions.Value;
            _multiBindingTcpListener = new MultiBindingTcpListener(options.ServerAddress, options.Port, logger);
        }

        /// <inheritdoc />
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.ReadyToRun;

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.ReadyToRun)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _task = StartListeningAsync(
                _newClientWriter,
                _multiBindingTcpListener,
                _logger,
                _connectionClosedCts,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running && Status != ConnectionStatus.Stopped && Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, {ConnectionStatus.Stopped}, or {ConnectionStatus.Paused}, but was {Status}.");
            }

            _jobStopped.Cancel();

            return _task;
        }

        /// <inheritdoc />
        public Task PauseAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, but was {Status}.");
            }

            _jobPaused.Cancel();

            return _task;
        }

        /// <inheritdoc />
        public Task ContinueAsync(CancellationToken cancellationToken)
        {
            if (Status == ConnectionStatus.Stopped)
            {
                // Stay stopped!
                return Task.CompletedTask;
            }

            if (Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Paused}, but was {Status}.");
            }

            _jobPaused = new CancellationTokenSource();

            _task = StartListeningAsync(
                _newClientWriter,
                _multiBindingTcpListener,
                _logger,
                _connectionClosedCts,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        [NotNull]
        private static async Task StartListeningAsync(
            [NotNull] ChannelWriter<TcpClient> newClientWriter,
            MultiBindingTcpListener listener,
            ILogger logger,
            CancellationTokenSource connectionClosedCts,
            CancellationToken jobStopped,
            CancellationToken jobPaused,
            IProgress<ConnectionStatus> statusProgress)
        {
            var globalCts = CancellationTokenSource.CreateLinkedTokenSource(connectionClosedCts.Token, jobStopped, jobPaused);

            Exception exception = null;
            try
            {
                await listener.StartAsync().ConfigureAwait(false);
                listener.StartAccepting();

                statusProgress.Report(ConnectionStatus.Running);

                try
                {
                    while (true)
                    {
                        var client = await listener.WaitAnyTcpClientAsync(globalCts.Token)
                           .ConfigureAwait(false);
                        await newClientWriter.WriteAsync(client, connectionClosedCts.Token)
                           .ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (ex.IsOperationCancelledException())
                {
                    // Ignore - everything is fine
                }
                finally
                {
                    listener.Stop();
                }
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "{0}", ex.Message);
                exception = ex;
            }

            // Don't call Complete() when the job was just paused.
            if (jobPaused.IsCancellationRequested)
            {
                statusProgress.Report(ConnectionStatus.Paused);
                return;
            }

            // Tell the channel that there's no more data coming
            newClientWriter.Complete(exception);

            // Signal a closed connection.
            connectionClosedCts.Cancel();

            // Change the status
            statusProgress.Report(ConnectionStatus.Stopped);
        }
    }
}
