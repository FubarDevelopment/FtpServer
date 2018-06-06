// <copyright file="IFtpServerStatistics.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Statistics about the FTP server.
    /// </summary>
    public interface IFtpServerStatistics
    {
        /// <summary>
        /// Gets the total number of connections.
        /// </summary>
        long TotalConnections { get; }

        /// <summary>
        /// Gets the currently active number of connections.
        /// </summary>
        long ActiveConnections { get; }
    }
}
