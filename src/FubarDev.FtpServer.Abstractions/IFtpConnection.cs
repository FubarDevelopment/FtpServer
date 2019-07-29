//-----------------------------------------------------------------------
// <copyright file="IFtpConnection.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface for an FTP connection.
    /// </summary>
    [Obsolete]
    public interface IFtpConnection : IFtpConnectionContext
    {
        /// <summary>
        /// Gets or sets the event handler that is triggered when the connection is closed.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Starts processing of messages for this connection.
        /// </summary>
        /// <returns>The task.</returns>
        Task StartAsync();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns>The task.</returns>
        Task StopAsync();
    }
}
