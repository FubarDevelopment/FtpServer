// <copyright file="FtpServerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The FTP server options.
    /// </summary>
    public class FtpServerOptions
    {
        /// <summary>
        /// Gets or sets the address the server listens on.
        /// Leave empty to listen on all interfaces.
        /// </summary>
        public string? ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the server port.
        /// </summary>
        public int Port { get; set; } = 21;

        /// <summary>
        /// Gets or sets the max allows active connections.
        /// </summary>
        /// <remarks>
        /// This will cause connections to be refused if count is exceeded.
        /// 0 (default) means no control over connection count.
        /// </remarks>
        public int MaxActiveConnections { get; set; }

        /// <summary>
        /// Gets or sets the interval between checks for inactive connections.
        /// </summary>
        public TimeSpan? ConnectionInactivityCheckInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets minimum port number to use for passive ftp.
        /// Only active if PasvMaxPort is set, too).
        /// If set, needs to be larger than 1023.
        /// </summary>
        public int PasvMinPort { get; set; }

        /// <summary>
        /// Gets or sets maximum port number to use for passive ftp.
        /// If set, needs to be larger than PasvMinPort.
        /// </summary>
        public int PasvMaxPort { get; set; }

        /// <summary>
        /// Gets or sets the address published to clients for PASV connections.
        /// This may be necessary if you are behind a forwarding firewall, for example.
        /// </summary>
        public string PasvAddress { get; set; }
    }
}
