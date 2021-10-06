// <copyright file="FtpConnectionIdleCheckSocketStateOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// Configures the check that tests if a socket is usable.
    /// </summary>
    public class FtpConnectionIdleCheckSocketStateOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this check is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
