// <copyright file="FtpConnectionDataTransferStoppedEvent.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Events
{
    /// <summary>
    /// This event object gets sent when a data transfer was stopped.
    /// </summary>
    public class FtpConnectionDataTransferStoppedEvent : IFtpConnectionEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionDataTransferStoppedEvent"/> class.
        /// </summary>
        /// <param name="transferId">The transfer ID.</param>
        public FtpConnectionDataTransferStoppedEvent(string transferId)
        {
            TransferId = transferId;
        }

        /// <summary>
        /// Gets the transfer ID.
        /// </summary>
        public string TransferId { get; }
    }
}
