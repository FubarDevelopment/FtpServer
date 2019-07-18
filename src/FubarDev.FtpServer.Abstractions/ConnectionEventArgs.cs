// <copyright file="ConnectionEventArgs.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Event arguments for a connection event.
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The connection of the event.</param>
        public ConnectionEventArgs(IFtpConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Gets the connection for this event.
        /// </summary>
        public IFtpConnection Connection { get; }
    }
}
