// <copyright file="PasvListenerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// IP address and port range to be used by the <c>PASV</c>/<c>EPSV</c> commands.
    /// </summary>
    public class PasvListenerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasvListenerOptions"/> class.
        /// </summary>
        /// <param name="minPort">The minimum port number.</param>
        /// <param name="maxPort">The maximum port number.</param>
        /// <param name="publicAddress">The public IP address.</param>
        public PasvListenerOptions(int minPort, int maxPort, IPAddress publicAddress)
        {
            PasvMinPort = minPort;
            PasvMaxPort = maxPort;
            PublicAddress = publicAddress;
        }

        /// <summary>
        /// Gets a value indicating whether the port range is configured.
        /// </summary>
        public bool HasPortRange => PasvMinPort != 0 || PasvMaxPort != 0;

        /// <summary>
        /// Gets the minimum port number to use for passive ftp.
        /// </summary>
        /// <remarks>
        /// Needs to be larger than 1023.
        /// </remarks>
        public int PasvMinPort { get; }

        /// <summary>
        /// Gets the maximum port number to use for passive ftp.
        /// </summary>
        public int PasvMaxPort { get; }

        /// <summary>
        /// Gets the address published to clients for PASV connections.
        /// </summary>
        public IPAddress PublicAddress { get; }
    }
}
