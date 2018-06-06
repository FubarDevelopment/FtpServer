// <copyright file="FtpConnectionOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Options for the FTP connection.
    /// </summary>
    public class FtpConnectionOptions
    {
        /// <summary>
        /// Gets or sets the default connection encoding.
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.ASCII;
    }
}
