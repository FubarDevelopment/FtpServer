// <copyright file="FtpConnectionIdleCheckOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for idle checks for connections.
    /// </summary>
    public class FtpConnectionIdleCheckOptions
    {
        /// <summary>
        /// Configuration for idle-by-inactivity check.
        /// </summary>
        public FtpConnectionIdleCheckInactivityOptions Inactivity { get; set; } = new FtpConnectionIdleCheckInactivityOptions();

        /// <summary>
        /// Configuration for idly-by-unusable-socket check.
        /// </summary>
        public FtpConnectionIdleCheckSocketStateOptions SocketState { get; set; } = new FtpConnectionIdleCheckSocketStateOptions();
    }
}
