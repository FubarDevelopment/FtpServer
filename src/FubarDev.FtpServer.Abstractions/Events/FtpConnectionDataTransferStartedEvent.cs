// <copyright file="FtpConnectionDataTransferStartedEvent.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Events
{
    /// <summary>
    /// This event object gets sent when a data transfer was started.
    /// </summary>
    public class FtpConnectionDataTransferStartedEvent : IFtpConnectionEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionDataTransferStartedEvent"/> class.
        /// </summary>
        /// <param name="transferId">The transfer ID.</param>
        public FtpConnectionDataTransferStartedEvent(string transferId)
        {
            TransferId = transferId;
        }

        /// <summary>
        /// Gets the transfer ID.
        /// </summary>
        public string TransferId { get; }
    }
}
