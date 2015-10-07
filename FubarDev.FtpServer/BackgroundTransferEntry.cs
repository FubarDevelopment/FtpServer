//-----------------------------------------------------------------------
// <copyright file="BackgroundTransferEntry.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    internal class BackgroundTransferEntry
    {
        public BackgroundTransferEntry([NotNull] IBackgroundTransfer backgroundTransfer, [CanBeNull] IFtpLog log)
        {
            BackgroundTransfer = backgroundTransfer;
            Log = log;
            Status = BackgroundTransferStatus.Enqueued;
        }

        [NotNull]
        public IBackgroundTransfer BackgroundTransfer { get; }

        [CanBeNull]
        public IFtpLog Log { get; }

        public BackgroundTransferStatus Status { get; set; }
    }
}
