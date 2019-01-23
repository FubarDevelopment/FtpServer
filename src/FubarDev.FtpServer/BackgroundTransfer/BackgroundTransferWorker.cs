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
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    internal class BackgroundTransferWorker : IBackgroundTransferWorker, IFtpService, IDisposable
    {
        private readonly ManualResetEvent _event = new ManualResetEvent(false);

        [CanBeNull]
        private readonly ILogger _log;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Semaphore that gets released when the queue stopped.
        /// </summary>
        private readonly SemaphoreSlim _stoppedSemaphore = new SemaphoreSlim(0, 1);

        private bool _disposedValue;

        public BackgroundTransferWorker(ILogger<BackgroundTransferWorker> logger = null)
        {
            _log = logger;
            Queue = new BackgroundTransferQueue(_event);
        }

        private BackgroundTransferQueue Queue { get; }

        private BackgroundTransferEntry CurrentEntry { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log?.LogTrace("Background transfer worker starting");
            Task.Run(() => ProcessQueue(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel(true);
            _log?.LogTrace("Background transfer worker stopping");
            return _stoppedSemaphore.WaitAsync(cancellationToken);
        }

        public void Enqueue(IBackgroundTransfer backgroundTransfer)
        {
            lock (Queue)
            {
                Queue.Enqueue(new BackgroundTransferEntry(backgroundTransfer));
            }
        }

        public IReadOnlyCollection<BackgroundTransferInfo> GetStates()
        {
            var result = new List<BackgroundTransferInfo>();
            lock (Queue)
            {
                var current = CurrentEntry;
                if (current != null)
                {
                    result.Add(GetInfo(current));
                }

                result.AddRange(Queue.GetEntries().Select(GetInfo));
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Cancel();
                }

                _disposedValue = true;
            }
        }

        private static BackgroundTransferInfo GetInfo(BackgroundTransferEntry entry)
        {
            return new BackgroundTransferInfo(entry.Status, entry.BackgroundTransfer.TransferId, entry.Transferred);
        }

        private BackgroundTransferEntry GetNextEntry()
        {
            lock (Queue)
            {
                var item = Queue.Dequeue();
                CurrentEntry = item;
                return item;
            }
        }

        private void ProcessQueue(CancellationToken cancellationToken)
        {
            var handles = new[]
            {
                cancellationToken.WaitHandle,
                _event,
            };

            _log?.LogDebug("Background transfer worker started.");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var handleIndex = WaitHandle.WaitAny(handles);
                    if (handleIndex == 0)
                    {
                        break;
                    }

                    BackgroundTransferEntry backgroundTransferEntry;
                    while ((backgroundTransferEntry = GetNextEntry()) != null)
                    {
                        using (_log?.BeginScope(
                            new Dictionary<string, object>
                            {
                                ["TransferId"] = backgroundTransferEntry.BackgroundTransfer.TransferId,
                            }))
                        {
                            var backgroundTransfer = backgroundTransferEntry.BackgroundTransfer;
                            try
                            {
                                var bt = backgroundTransfer;
                                _log?.LogInformation("Starting background transfer {0}", bt.TransferId);
                                backgroundTransferEntry.Status = BackgroundTransferStatus.Transferring;

                                // ReSharper disable once AccessToModifiedClosure
                                var progress = new ActionProgress(sent => backgroundTransferEntry.Transferred = sent);
                                var task = bt.Start(progress, cancellationToken);
                                var cancelledTask = task
                                    .ContinueWith(
                                        t =>
                                        {
                                            // Nothing to do
                                            _log?.LogWarning("Background transfer {0} cancelled", bt.TransferId);
                                        },
                                        TaskContinuationOptions.OnlyOnCanceled);
                                var faultedTask = task
                                    .ContinueWith(
                                        t =>
                                        {
                                            _log?.LogError(
                                                t.Exception,
                                                "Background transfer {0} faulted",
                                                bt.TransferId);
                                        },
                                        TaskContinuationOptions.OnlyOnFaulted);
                                var completedTask = task
                                    .ContinueWith(
                                        t =>
                                        {
                                            // Nothing to do
                                            _log?.LogInformation("Completed background transfer {0}", bt.TransferId);
                                        },
                                        TaskContinuationOptions.NotOnCanceled);

                                try
                                {
                                    Task.WaitAll(cancelledTask, faultedTask, completedTask);
                                }
                                catch (AggregateException ex) when (ex.InnerExceptions.All(
                                    x => x is TaskCanceledException))
                                {
                                    // Ignore AggregateException when it only contains TaskCancelledException
                                }

                                _log?.LogTrace("Background transfer {0} finished", bt.TransferId);
                            }
                            catch (Exception ex)
                            {
                                _log?.LogError(
                                    ex,
                                    "Error during execution of background transfer {0}",
                                    backgroundTransfer.TransferId);
                            }
                            finally
                            {
                                backgroundTransfer.Dispose();
                            }
                        }

                        backgroundTransferEntry.Status = BackgroundTransferStatus.Finished;
                        CurrentEntry = null;
                    }
                }

                _log?.LogInformation("Cancellation requested - stopping background transfer worker.");
            }
            finally
            {
                _log?.LogDebug("Background transfer worker stopped.");
                Queue.Dispose();
                _stoppedSemaphore.Release();
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
