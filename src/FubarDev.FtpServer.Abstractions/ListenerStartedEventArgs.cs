// <copyright file="ListenerStartedEventArgs.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Event arguments for a started listener.
    /// </summary>
    public class ListenerStartedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListenerStartedEventArgs"/> class.
        /// </summary>
        /// <param name="port">The port used by the listener.</param>
        public ListenerStartedEventArgs(int port)
        {
            Port = port;
        }

        /// <summary>
        /// Gets the port that's used by the listener.
        /// </summary>
        public int Port { get; }
    }
}
