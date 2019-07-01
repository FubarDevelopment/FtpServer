// <copyright file="FtpServerStatistics.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Statistics about the FTP server.
    /// </summary>
    internal class FtpServerStatistics : IFtpServerStatistics
    {
        private long _totalConnections;
        private long _activeConnections;

        /// <inheritdoc />
        public long TotalConnections => _totalConnections;

        /// <inheritdoc />
        public long ActiveConnections => _activeConnections;

        public void AddConnection()
        {
            Interlocked.Increment(ref _totalConnections);
            Interlocked.Increment(ref _activeConnections);
        }

        public void CloseConnection()
        {
            Interlocked.Decrement(ref _activeConnections);
        }
    }
}
