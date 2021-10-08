// <copyright file="IFtpListenerService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Service to control the listener for FTP connections.
    /// </summary>
    public interface IFtpListenerService : IPausableFtpService
    {
        /// <summary>
        /// Event to be triggered when the listener started.
        /// </summary>
        event EventHandler<ListenerStartedEventArgs>? ListenerStarted;

        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> for the listener.
        /// </summary>
        CancellationTokenSource ListenerShutdown { get; }

        /// <summary>
        /// Gets the channel with new TCP clients.
        /// </summary>
        ChannelReader<TcpClient> Channel { get; }
    }
}
