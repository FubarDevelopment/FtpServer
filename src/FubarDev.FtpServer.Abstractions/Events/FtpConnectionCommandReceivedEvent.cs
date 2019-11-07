// <copyright file="FtpConnectionCommandReceivedEvent.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Events
{
    /// <summary>
    /// This event object gets sent when a command was received.
    /// </summary>
    public class FtpConnectionCommandReceivedEvent : IFtpConnectionEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionCommandReceivedEvent"/> class.
        /// </summary>
        /// <param name="command">The received FTP command.</param>
        public FtpConnectionCommandReceivedEvent(FtpCommand command)
        {
            Command = command;
        }

        /// <summary>
        /// Gets the received FTP command.
        /// </summary>
        public FtpCommand Command { get; }
    }
}
