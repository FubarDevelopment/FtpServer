// <copyright file="FtpServerPasvOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.Configuration
{
    /// <summary>
    /// The options for PASV/EPSV.
    /// </summary>
    public class FtpServerPasvOptions
    {
        /// <summary>
        /// Gets or sets the port range.
        /// </summary>
        public string? Range { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether promiscuous PASV is allowed.
        /// </summary>
        public bool Promiscuous { get; set; }
    }
}
