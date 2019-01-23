// <copyright file="IFtpServer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The interface that must be implemented by the FTP server
    /// </summary>
    public interface IFtpServer
    {
        /// <summary>
        /// This event is raised when the connection is ready to be configured.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ConfigureConnection;

        /// <summary>
        /// Gets the public IP address (required for <code>PASV</code> and <code>EPSV</code>).
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
    }
}
