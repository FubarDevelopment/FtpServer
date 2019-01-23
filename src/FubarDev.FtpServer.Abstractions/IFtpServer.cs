// <copyright file="IFtpServer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface that must be implemented by the FTP server.
    /// </summary>
    public interface IFtpServer
    {
        /// <summary>
        /// This event is raised when the connection is ready to be configured.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ConfigureConnection;

        /// <summary>
        /// Gets the public IP address (required for <c>PASV</c> and <c>EPSV</c>).
        /// </summary>
        [NotNull]
        string ServerAddress { get; }

        /// <summary>
        /// Gets the port on which the FTP server is listening for incoming connections.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets a value indicating whether server ready to receive incoming connectoions.
        /// </summary>
        bool Ready { get; }

        /// <summary>
        /// Gets the FTP server statistics.
        /// </summary>
        [NotNull]
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
