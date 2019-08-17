//-----------------------------------------------------------------------
// <copyright file="BackgroundTransferWorker.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    internal class BackgroundTransferWorker : IBackgroundTransferWorker, IFtpService, IDisposable
    {
        private readonly ILogger? _log;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Channel<BackgroundTransferEntry> _channel;

        private readonly Dictionary<Guid, BackgroundTransferEntry> _pendingOrActiveEntries = new Dictionary<Guid, BackgroundTransferEntry>();

        private Task? _processQueueTask;

        private long _sequenceNumber;

        private bool _disposedValue;

        public BackgroundTransferWorker(ILogger<BackgroundTransferWorker>? logger = null)
        {
            _log = logger;
            _channel = Channel.CreateUnbounded<BackgroundTransferEntry>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log?.LogTrace("Background transfer worker starting");
            _processQueueTask = ProcessQueueAsync(_channel, _cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel(true);
            _log?.LogTrace("Background transfer worker stopping");

            if (_processQueueTask != null)
            {
                await _processQueueTask.ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task EnqueueAsync(IBackgroundTransfer backgroundTransfer, CancellationToken cancellationToken)
        {
            var sequenceNumber = Interlocked.Increment(ref _sequenceNumber);
            var newEntry = new BackgroundTransferEntry(backgroundTransfer, sequenceNumber);
            lock (_pendingOrActiveEntries)
            {
                _pendingOrActiveEntries.Add(newEntry.Id, newEntry);
            }

            await _channel.Writer
               .WriteAsync(newEntry, cancellationToken)
               .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<BackgroundTransferInfo> GetStates()
        {
            var result = new List<BackgroundTransferInfo>();
            lock (_pendingOrActiveEntries)
            {
                result.AddRange(_pendingOrActiveEntries.Values.OrderBy(x => x.SequenceNumber).Select(GetInfo));
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private static BackgroundTransferInfo GetInfo(BackgroundTransferEntry entry)
        {
            return new BackgroundTransferInfo(entry.Status, entry.BackgroundTransfer.TransferId, entry.Transferred);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                _disposedValue = true;
            }
        }

        private async Task ProcessQueueAsync(
            ChannelReader<BackgroundTransferEntry> reader,
            CancellationToken cancellationToken)
        {
            _log?.LogTrace("Background transfer worker started");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var entry = await reader.ReadAsync(cancellationToken)
                       .ConfigureAwait(false);
                    await ExecuteAsync(entry, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex.Is<OperationCanceledException>())
            {
                // Ignore. It's expected.
            }
            finally
            {
                // Cancel pending background transfers
                lock (_pendingOrActiveEntries)
                {
                    foreach (var entry in _pendingOrActiveEntries.Values)
                    {
                        entry.BackgroundTransfer.Dispose();
                    }

                    _pendingOrActiveEntries.Clear();
                }

                _log?.LogTrace("Background transfer worker stopped");
            }
        }

        private async Task ExecuteAsync(BackgroundTransferEntry backgroundTransferEntry, CancellationToken cancellationToken)
        {
            using (_log?.BeginScope(
                new Dictionary<string, object>
                {
                    ["TransferId"] = backgroundTransferEntry.BackgroundTransfer.TransferId,
                }))
            {
                var backgroundTransfer = backgroundTransferEntry.BackgroundTransfer;
                _log?.LogInformation("Starting background transfer {0}", backgroundTransfer.TransferId);
                backgroundTransferEntry.Status = BackgroundTransferStatus.Transferring;

                // ReSharper disable once AccessToModifiedClosure
                var progress = new ActionProgress(sent => backgroundTransferEntry.Transferred = sent);
                try
                {
                    await backgroundTransfer.Start(progress, cancellationToken)
                       .ConfigureAwait(false);
                    _log?.LogInformation("Completed background transfer {0}", backgroundTransfer.TransferId);
                }
                catch (Exception ex) when (ex.Is<OperationCanceledException>())
                {
                    // Nothing to do
                    _log?.LogWarning("Background transfer {0} cancelled", backgroundTransfer.TransferId);
                }
                catch (Exception ex)
                {
                    // Show the error message
                    _log?.LogError(ex, "Background transfer {0} faulted", backgroundTransfer.TransferId);
                }
                finally
                {
                    lock (_pendingOrActiveEntries)
                    {
                        _pendingOrActiveEntries.Remove(backgroundTransferEntry.Id);
                    }

                    backgroundTransferEntry.Status = BackgroundTransferStatus.Finished;
                    backgroundTransfer.Dispose();
                }
            }
        }

        private class ActionProgress : IProgress<long>
        {
            private readonly Action<long> _progressAction;

            public ActionProgress(Action<long> progressAction)
            {
                _progressAction = progressAction;
            }

            public void Report(long value)
            {
                _progressAction(value);
            }
        }
    }
}
