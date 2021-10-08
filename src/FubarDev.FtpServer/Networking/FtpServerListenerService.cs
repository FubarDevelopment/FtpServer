// <copyright file="FtpServerListenerService.cs" company="Fubar Development Junker">
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
    /// Listener for the server.
    /// </summary>
    /// <remarks>
    /// Accepting connections can be paused.
    /// </remarks>
    internal sealed class FtpServerListenerService : PausableFtpService
    {
        private readonly ChannelWriter<TcpClient> _newClientWriter;
        private readonly MultiBindingTcpListener _multiBindingTcpListener;
        private readonly CancellationTokenSource _connectionClosedCts;

        private Exception? _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServerListenerService"/> class.
        /// </summary>
        /// <param name="newClientWriter">Channel that receives all accepted clients.</param>
        /// <param name="serverOptions">The server options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="connectionClosedCts">Cancellation token source for a closed connection.</param>
        public FtpServerListenerService(
            ChannelWriter<TcpClient> newClientWriter,
            IOptions<FtpServerOptions> serverOptions,
            CancellationTokenSource connectionClosedCts,
            ILogger? logger = null)
            : base(connectionClosedCts.Token, logger)
        {
            _newClientWriter = newClientWriter;
            _connectionClosedCts = connectionClosedCts;
            var options = serverOptions.Value;
            _multiBindingTcpListener = new MultiBindingTcpListener(options.ServerAddress, options.Port, logger);
        }

        /// <summary>
        /// Event for a started listener.
        /// </summary>
        public event EventHandler<ListenerStartedEventArgs>? ListenerStarted;

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _multiBindingTcpListener.StartAsync().ConfigureAwait(false);

            // Notify of the port that's used by the listener.
            OnListenerStarted(new ListenerStartedEventArgs(_multiBindingTcpListener.Port));

            _multiBindingTcpListener.StartAccepting();

            try
            {
                while (true)
                {
                    var client = await _multiBindingTcpListener.WaitAnyTcpClientAsync(cancellationToken)
                       .ConfigureAwait(false);
                    await _newClientWriter.WriteAsync(client, cancellationToken)
                       .ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex.Is<OperationCanceledException>())
            {
                // Ignore - everything is fine
            }
            finally
            {
                _multiBindingTcpListener.Stop();
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> OnFailedAsync(Exception exception, CancellationToken cancellationToken)
        {
            await base.OnFailedAsync(exception, cancellationToken)
               .ConfigureAwait(false);

            _exception = exception;

            return true;
        }

        /// <inheritdoc />
        protected override Task OnStoppedAsync(CancellationToken cancellationToken)
        {
            if (!_connectionClosedCts.IsCancellationRequested)
            {
                // Tell the channel that there's no more data coming
                _newClientWriter.Complete(_exception);

                // Signal a closed connection.
                _connectionClosedCts.Cancel();
            }

            return Task.CompletedTask;
        }

        private void OnListenerStarted(ListenerStartedEventArgs e)
        {
            ListenerStarted?.Invoke(this, e);
        }
    }
}
