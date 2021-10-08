// <copyright file="IFtpServer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface that must be implemented by the FTP server.
    /// </summary>
    public interface IFtpServer : IPausableFtpService
    {
        /// <summary>
        /// This event is raised when the connection is ready to be configured.
        /// </summary>
        [Obsolete("Register a service of type IFtpConnectionConfigurator instead.")]
        event EventHandler<ConnectionEventArgs>? ConfigureConnection;

        /// <summary>
        /// This event is raised when the listener was started.
        /// </summary>
        event EventHandler<ListenerStartedEventArgs>? ListenerStarted;

        /// <summary>
        /// Gets the public IP address (required for <c>PASV</c> and <c>EPSV</c>).
        /// </summary>
        string? ServerAddress { get; }

        /// <summary>
        /// Gets the port on which the FTP server is listening for incoming connections.
        /// </summary>
        /// <remarks>
        /// This value is only final after the <see cref="ListenerStarted"/> event was raised.
        /// </remarks>
        int Port { get; }

        /// <summary>
        /// Gets the max allows active connections.
        /// </summary>
        /// <remarks>
        /// This will cause connections to be refused if count is exceeded.
        /// 0 (default) means no control over connection count.
        /// </remarks>
        int MaxActiveConnections { get; }

        /// <summary>
        /// Gets a value indicating whether server ready to receive incoming connections.
        /// </summary>
        bool Ready { get; }

        /// <summary>
        /// Gets the FTP server statistics.
        /// </summary>
        IFtpServerStatistics Statistics { get; }

        /// <summary>
        /// Starts the FTP server in the background.
        /// </summary>
        [Obsolete("User IFtpServerHost.StartAsync instead.")]
        void Start();

        /// <summary>
        /// Stops the FTP server.
        /// </summary>
        /// <remarks>
        /// The FTP server cannot be started again after it was stopped.
        /// </remarks>
        [Obsolete("User IFtpServerHost.StopAsync instead.")]
        void Stop();
    }
}
