// <copyright file="ConnectionEventArgs.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Event arguments for a connection event.
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        private readonly List<ConnectionInitAsyncDelegate> _asyncInitFunctions = new List<ConnectionInitAsyncDelegate>();

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

        /// <summary>
        /// Gets the list of async init functions.
        /// </summary>
        public IEnumerable<ConnectionInitAsyncDelegate> AsyncInitFunctions => _asyncInitFunctions;

        /// <summary>
        /// Adds a new async init function.
        /// </summary>
        /// <param name="asyncInitFunc">The async init function to add.</param>
        public void AddAsyncInit(ConnectionInitAsyncDelegate asyncInitFunc)
        {
            _asyncInitFunctions.Add(asyncInitFunc);
        }
    }
}
