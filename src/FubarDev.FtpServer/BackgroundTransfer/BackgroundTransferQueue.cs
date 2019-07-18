//-----------------------------------------------------------------------
// <copyright file="BackgroundTransferQueue.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    internal class BackgroundTransferQueue : IDisposable
    {
        private readonly object _syncRoot = new object();

        private readonly Queue<BackgroundTransferEntry> _queue = new Queue<BackgroundTransferEntry>();

        private readonly ManualResetEvent _event;

        private bool _disposedValue;

        internal BackgroundTransferQueue(ManualResetEvent evt)
        {
            _event = evt;
        }

        public IReadOnlyCollection<BackgroundTransferEntry> GetEntries()
        {
            lock (_syncRoot)
            {
                var result = new List<BackgroundTransferEntry>(_queue.Count);
                result.AddRange(_queue);
                return result;
            }
        }

        public void Enqueue(BackgroundTransferEntry transfer)
        {
            lock (_syncRoot)
            {
                if (_disposedValue)
                {
                    throw new ObjectDisposedException("_queue");
                }

                _queue.Enqueue(transfer);
                _event.Set();
            }
        }

        public BackgroundTransferEntry? Dequeue()
        {
            lock (_syncRoot)
            {
                if (_queue.Count == 0)
                {
                    return null;
                }

                var result = _queue.Dequeue();
                if (_queue.Count == 0)
                {
                    _event.Reset();
                }

                return result;
            }
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
                    List<BackgroundTransferEntry> transfers;
                    lock (_syncRoot)
                    {
                        transfers = new List<BackgroundTransferEntry>(_queue);
                        _queue.Clear();
                    }
                    foreach (var transfer in transfers)
                    {
                        transfer.BackgroundTransfer.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }
    }
}
