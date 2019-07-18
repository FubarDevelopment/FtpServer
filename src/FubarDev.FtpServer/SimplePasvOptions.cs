// <copyright file="SimplePasvOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Options for the <see cref="SimplePasvAddressResolver"/>.
    /// </summary>
    public class SimplePasvOptions
    {
        /// <summary>
        /// Gets or sets minimum port number to use for passive ftp.
        /// </summary>
        /// <remarks>
        /// Only active if PasvMaxPort is set, too).
        /// If set, needs to be larger than 1023.
        /// </remarks>
        public int? PasvMinPort { get; set; }

        /// <summary>
        /// Gets or sets maximum port number to use for passive ftp.
        /// </summary>
        /// <remarks>
        /// If set, needs to be larger than PasvMinPort.
        /// </remarks>
        public int? PasvMaxPort { get; set; }

        /// <summary>
        /// Gets or sets the address published to clients for PASV connections.
        /// </summary>
        /// <remarks>
        /// This may be necessary if you are behind a forwarding firewall, for example.
        /// </remarks>
        public IPAddress? PublicAddress { get; set; }
    }
}
