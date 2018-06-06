// <copyright file="FtpServerStatistics.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Statistics about the FTP server.
    /// </summary>
    public class FtpServerStatistics
    {
        /// <summary>
        /// Gets the total number of connections.
        /// </summary>
        public long TotalConnections
        { get; internal set; }

        /// <summary>
        /// Gets the currently active number of connections.
        /// </summary>
        public long ActiveConnections
        { get; internal set; }
    }
}
