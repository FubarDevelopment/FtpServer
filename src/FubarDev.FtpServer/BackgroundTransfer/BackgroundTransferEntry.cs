//-----------------------------------------------------------------------
// <copyright file="BackgroundTransferEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    internal class BackgroundTransferEntry
    {
        private readonly object _sync = new object();
        private long? _transferred;

        public BackgroundTransferEntry(
            IBackgroundTransfer backgroundTransfer,
            long sequenceNumber)
        {
            BackgroundTransfer = backgroundTransfer;
            SequenceNumber = sequenceNumber;
            Status = BackgroundTransferStatus.Enqueued;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public IBackgroundTransfer BackgroundTransfer { get; }

        public BackgroundTransferStatus Status { get; set; }

        public long SequenceNumber { get; }

        public long? Transferred
        {
            get
            {
                lock (_sync)
                {
                    return _transferred;
                }
            }
            set
            {
                lock (_sync)
                {
                    _transferred = value;
                }
            }
        }
    }
}
