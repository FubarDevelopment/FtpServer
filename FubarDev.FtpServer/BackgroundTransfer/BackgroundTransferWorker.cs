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

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    internal class BackgroundTransferWorker : IDisposable
    {
        private readonly ManualResetEvent _event = new ManualResetEvent(false);

        private readonly ILogger _log;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _disposedValue;

        public BackgroundTransferWorker(ILogger<BackgroundTransferWorker> logger)
        {
            _log = logger;
            Queue = new BackgroundTransferQueue(_event);
        }

        public BackgroundTransferQueue Queue { get; }

        public bool HasData { get; private set; }

        internal BackgroundTransferEntry CurrentEntry { get; private set; }

        public Task Start(CancellationTokenSource cts)
        {
            if (_cancellationTokenSource != null)
                throw new InvalidOperationException("Background transfer worker already started");
            _cancellationTokenSource = cts;
            return Task.Run(() => ProcessQueue(cts.Token), cts.Token);
        }

        public void Enqueue(BackgroundTransferEntry entry)
        {
            lock (Queue)
            {
                Queue.Enqueue(entry);
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

            _log?.LogDebug("Starting background transfer worker.");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var handleIndex = WaitHandle.WaitAny(handles);
                    if (handleIndex == 0)
                        break;

                    HasData = true;

                    try
                    {
                        BackgroundTransferEntry backgroundTransferEntry;
                        while ((backgroundTransferEntry = GetNextEntry()) != null)
                        {
                            var log = backgroundTransferEntry.Log;
                            var backgroundTransfer = backgroundTransferEntry.BackgroundTransfer;
                            try
                            {
                                var bt = backgroundTransfer;
                                log?.LogInformation("Starting background transfer {0}", bt.TransferId);
                                backgroundTransferEntry.Status = BackgroundTransferStatus.Transferring;
                                var progress = new ActionProgress(sent => backgroundTransferEntry.Transferred = sent);
                                var task = bt.Start(progress, cancellationToken);
                                var cancelledTask = task
                                    .ContinueWith(
                                        t =>
                                        {
                                            // Nothing to do
                                            log?.LogWarning("Background transfer {0} cancelled", bt.TransferId);
                                        },
                                        TaskContinuationOptions.OnlyOnCanceled);
                                var faultedTask = task
                                    .ContinueWith(
                                        t =>
                                        {
                                            log?.LogError(t.Exception, "Background transfer {0} faulted", bt.TransferId);
                                        },
                                        TaskContinuationOptions.OnlyOnFaulted);
                                var completedTask = task
                                    .ContinueWith(
                                        t =>
                                        {
                                            // Nothing to do
                                            log?.LogInformation("Completed background transfer {0}", bt.TransferId);
                                        },
                                        TaskContinuationOptions.NotOnCanceled);

                                try
                                {
                                    Task.WaitAll(cancelledTask, faultedTask, completedTask);
                                }
                                catch (AggregateException ex) when (ex.InnerExceptions.All(x => x is TaskCanceledException))
                                {
                                    // Ignore AggregateException when it only contains TaskCancelledException
                                }

                                log?.LogTrace("Background transfer {0} finished", bt.TransferId);
                            }
                            catch (Exception ex)
                            {
                                log?.LogError(ex, "Error during execution of background transfer {0}", backgroundTransfer.TransferId);
                            }
                            finally
                            {
                                backgroundTransfer.Dispose();
                            }

                            backgroundTransferEntry.Status = BackgroundTransferStatus.Finished;
                            CurrentEntry = null;
                        }
                    }
                    finally
                    {
                        HasData = false;
                    }
                }
                _log?.LogInformation("Cancellation requested - stopping background transfer worker.");
            }
            finally
            {
                _log?.LogDebug("Background transfer worker stopped.");
                Queue.Dispose();
            }
        }

        private static BackgroundTransferInfo GetInfo(BackgroundTransferEntry entry)
        {
            return new BackgroundTransferInfo(entry.Status, entry.BackgroundTransfer.TransferId, entry.Transferred);
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
