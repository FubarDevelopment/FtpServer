// <copyright file="HostInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Sockets;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Record for a host name/address.
    /// </summary>
    public class HostInfo
    {
        private readonly string _hostName;
        private readonly IPAddress _address;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostInfo"/> class.
        /// </summary>
        public HostInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostInfo"/> class.
        /// </summary>
        /// <param name="address">The IP address.</param>
        public HostInfo([NotNull] IPAddress address)
        {
            _address = address;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostInfo"/> class.
        /// </summary>
        /// <param name="hostName">The host name.</param>
        public HostInfo([NotNull] string hostName)
        {
            _hostName = hostName;
        }

        /// <summary>
        /// Gets a value indicating whether this host object is empty.
        /// </summary>
        public bool IsEmpty => _address == null && _hostName == null;

        /// <summary>
        /// Gets a value indicating whether this object represents an IP address.
        /// </summary>
        public bool IsAddress => _address != null;

        /// <summary>
        /// Gets the IP address.
        /// </summary>
        public IPAddress Address => _address ?? throw new InvalidOperationException("No address stored");

        /// <summary>
        /// Gets the host name.
        /// </summary>
        public string HostName => _hostName ?? throw new InvalidOperationException("No host name stored");

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsEmpty)
            {
                return string.Empty;
            }

            if (!IsAddress)
            {
                return _hostName;
            }

            if (Address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return $"[{Address}]";
            }

            return Address.ToString();
        }
    }
}
