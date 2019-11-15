// <copyright file="FtpConnectionStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Api
{
    /// <summary>
    /// Information about a single FTP connection.
    /// </summary>
    public class FtpConnectionStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionStatus"/> class.
        /// </summary>
        /// <param name="id">The ID of the connection.</param>
        /// <param name="remoteIp">The client IP.</param>
        public FtpConnectionStatus(string id, string remoteIp)
        {
            Id = id;
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets or sets the ID of the connection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is alive (read: not expired).
        /// </summary>
        public bool IsAlive { get; set; }

        /// <summary>
        /// Gets or sets the remote IP.
        /// </summary>
        public string RemoteIp { get; set; }

        /// <summary>
        /// Gets or sets the user of the connection.
        /// </summary>
        public FtpUser? User { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has an active file transfer.
        /// </summary>
        public bool HasActiveTransfer { get; set; }
    }
}
