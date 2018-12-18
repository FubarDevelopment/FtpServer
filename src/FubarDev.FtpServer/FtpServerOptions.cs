// <copyright file="FtpServerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The FTP server options.
    /// </summary>
    public class FtpServerOptions
    {
        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the server port.
        /// </summary>
        public int Port { get; set; } = 21;
    }
}
