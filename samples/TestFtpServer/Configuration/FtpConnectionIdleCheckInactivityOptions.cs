// <copyright file="FtpConnectionIdleCheckInactivityOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Options for manual idle checks for connections with an inactivity timeout.
    /// </summary>
    public class FtpConnectionIdleCheckInactivityOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this check is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the timeout for inactive connections.
        /// </summary>
        public int? InactivityTimeout { get; set; }
    }
}
