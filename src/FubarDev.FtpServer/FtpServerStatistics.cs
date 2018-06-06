// <copyright file="FtpServerStatistics.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Statistics about the FTP server.
    /// </summary>
    public class FtpServerStatistics : IFtpServerStatistics
    {
        /// <inheritdoc />
        public long TotalConnections { get; internal set; }

        /// <inheritdoc />
        public long ActiveConnections { get; internal set; }
    }
}
