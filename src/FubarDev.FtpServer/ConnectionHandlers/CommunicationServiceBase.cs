// <copyright file="CommunicationServiceBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public abstract class CommunicationServiceBase : IPausableCommunicationService
    {
        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        private readonly CancellationToken _connectionClosed;

        [NotNull]
        private CancellationTokenSource _jobPaused = new CancellationTokenSource();

        [NotNull]
        private Task _task = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationServiceBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="connectionClosed">Cancellation token source for a closed connection.</param>
        protected CommunicationServiceBase(
            CancellationToken connectionClosed,
            [CanBeNull] ILogger logger = null)
        {
            Logger = logger;
            _connectionClosed = connectionClosed;
        }

        /// <inheritdoc />
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.ReadyToRun;

        [CanBeNull]
        protected ILogger Logger { get; }

        protected bool IsConnectionClosed => _connectionClosed.IsCancellationRequested;

        protected bool IsStopRequested => _jobStopped.IsCancellationRequested;

        protected bool IsPauseRequested => _jobPaused.IsCancellationRequested;

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.ReadyToRun)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _task = RunAsync(new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running && Status != ConnectionStatus.Stopped && Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, {ConnectionStatus.Stopped}, or {ConnectionStatus.Paused}, but was {Status}.");
            }

            await OnStopRequestingAsync(cancellationToken)
               .ConfigureAwait(false);

            _jobStopped.Cancel();

            await OnStopRequestedAsync(cancellationToken)
               .ConfigureAwait(false);

            await _task
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task PauseAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, but was {Status}.");
            }

            await OnPauseRequestingAsync(cancellationToken)
               .ConfigureAwait(false);

            _jobPaused.Cancel();

            await OnPauseRequestedAsync(cancellationToken)
               .ConfigureAwait(false);

            await _task
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task ContinueAsync(CancellationToken cancellationToken)
        {
            if (Status == ConnectionStatus.Stopped)
            {
                // Stay stopped!
                return;
            }

            if (Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Paused}, but was {Status}.");
            }

            _jobPaused = new CancellationTokenSource();

            await OnContinueRequestingAsync(cancellationToken)
               .ConfigureAwait(false);

            _task = RunAsync(new Progress<ConnectionStatus>(status => Status = status));
        }

        [NotNull]
        protected abstract Task ExecuteAsync(
            CancellationToken cancellationToken);

        [NotNull]
        protected virtual Task OnStopRequestingAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [NotNull]
        protected virtual Task OnStopRequestedAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [NotNull]
        protected virtual Task OnPauseRequestingAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [NotNull]
        protected virtual Task OnPauseRequestedAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [NotNull]
        protected virtual Task OnContinueRequestingAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [NotNull]
        protected virtual Task OnPausedAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [NotNull]
        protected virtual Task OnStoppedAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [NotNull]
        protected virtual Task<bool> OnFailedAsync(
            [CanBeNull] Exception exception,
            CancellationToken cancellationToken)
        {
            Logger?.LogCritical(exception, exception.Message);
            return Task.FromResult(false);
        }

        private async Task RunAsync(
            [NotNull] IProgress<ConnectionStatus> statusProgress)
        {
            using (var globalCts = CancellationTokenSource.CreateLinkedTokenSource(
                _connectionClosed,
                _jobStopped.Token,
                _jobPaused.Token))
            {
                statusProgress.Report(ConnectionStatus.Running);

                try
                {
                    await ExecuteAsync(globalCts.Token)
                       .ConfigureAwait(false);
                }
                catch (Exception ex) when (ex.IsOperationCancelledException())
                {
                    // Ignore - everything is fine
                }
                catch (Exception ex) when (ex.IsIOException())
                {
                    // Ignore - everything is fine
                }
                catch (Exception ex)
                {
                    var isHandled = await OnFailedAsync(ex, _connectionClosed)
                       .ConfigureAwait(false);

                    if (!isHandled)
                    {
                        throw;
                    }
                }
            }

            // Don't call Complete() when the job was just paused.
            if (IsPauseRequested)
            {
                statusProgress.Report(ConnectionStatus.Paused);
                await OnPausedAsync(_connectionClosed)
                   .ConfigureAwait(false);
                return;
            }

            // Change the status
            statusProgress.Report(ConnectionStatus.Stopped);
            await OnStoppedAsync(_connectionClosed)
               .ConfigureAwait(false);
        }
    }
}
