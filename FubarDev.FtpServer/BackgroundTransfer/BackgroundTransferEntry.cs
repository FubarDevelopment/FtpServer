//-----------------------------------------------------------------------
// <copyright file="BackgroundTransferEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.BackgroundTransfer
{
    internal class BackgroundTransferEntry
    {
        private readonly object _sync = new object();
        private long? _transferred;

        public BackgroundTransferEntry([NotNull] IBackgroundTransfer backgroundTransfer, [CanBeNull] ILogger log)
        {
            BackgroundTransfer = backgroundTransfer;
            Log = log;
            Status = BackgroundTransferStatus.Enqueued;
        }

        [NotNull]
        public IBackgroundTransfer BackgroundTransfer { get; }

        [CanBeNull]
        public ILogger Log { get; }

        public BackgroundTransferStatus Status { get; set; }

        public long? Transferred
        {
            get
            {
                lock (_sync)
                    return _transferred;
            }
            set
            {
                lock (_sync)
                    _transferred = value;
            }
        }
    }
}
