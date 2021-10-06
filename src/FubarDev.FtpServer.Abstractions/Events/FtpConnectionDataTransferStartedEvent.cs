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
        /// <param name="command">The command that initiated the data transfer.</param>
        public FtpConnectionDataTransferStartedEvent(string transferId, FtpCommand command)
        {
            TransferId = transferId;
            Command = command;
        }

        /// <summary>
        /// Gets the transfer ID.
        /// </summary>
        public string TransferId { get; }

        /// <summary>
        /// Gets the command that initiated the transfer.
        /// </summary>
        public FtpCommand Command { get; }
    }
}
