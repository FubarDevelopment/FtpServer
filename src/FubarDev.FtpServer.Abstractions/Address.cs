//-----------------------------------------------------------------------
// <copyright file="Address.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Net;
using System.Text;

using AF = System.Net.Sockets.AddressFamily;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Abstraction for an IP address.
    /// </summary>
    [Obsolete("Use IPEndPoint instead")]
    public class Address
    {
        private readonly bool _isEnhanced;

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <param name="address">IPv4 address.</param>
        /// <param name="port">The port.</param>
        public Address(string? address, int port)
        {
            _isEnhanced = false;
            IPAddress = IPAddress.Parse(address);
            Port = port;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// This constructor omits the address part.
        /// </remarks>
        public Address(int port)
        {
            _isEnhanced = true;
            IPAddress = null;
            Port = port;
        }

        /// <summary>
        /// Gets the IP address family.
        /// </summary>
        public AF? AddressFamily => IPAddress?.AddressFamily;

        /// <summary>
        /// Gets the IP address.
        /// </summary>
        public IPAddress? IPAddress { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Parses an IP address.
        /// </summary>
        /// <param name="address">The IP address to parse.</param>
        /// <returns>The parsed IP address.</returns>
        public static Address? Parse(string? address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            return IsEnhancedAddress(address!)
                ? ParseEnhanced(address!)
                : ParseLegacy(address!);
        }

        /// <summary>
        /// Converts this address to an URI.
        /// </summary>
        /// <returns>The newly created URI.</returns>
        public Uri ToUri()
        {
            if (AddressFamily != null && AddressFamily == AF.InterNetworkV6)
            {
                return new Uri($"port://[{IPAddress}]:{Port}/");
            }
            return new Uri($"port://{IPAddress}:{Port}/");
        }

        /// <summary>
        /// Converts the IP address into a string.
        /// </summary>
        /// <param name="logFormat"><code>true</code> when it should be converted to a loggable format, otherwise the FTP format is used.</param>
        /// <returns>The IP address as string.</returns>
        public string ToString(bool logFormat)
        {
            if (logFormat)
            {
                if (AddressFamily != null && AddressFamily == AF.InterNetworkV6)
                {
                    return $"[{IPAddress}]:{Port}";
                }
                return $"{IPAddress}:{Port}";
            }
            return ToString();
        }

        /// <summary>
        /// Converts the IP address to a string as required by the PASV command.
        /// </summary>
        /// <returns>The IP address as string.</returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            if (_isEnhanced)
            {
                result.Append("|");
                if (AddressFamily != null)
                {
                    switch (AddressFamily)
                    {
                        case AF.InterNetwork:
                            result.Append("1");
                            break;
                        case AF.InterNetworkV6:
                            result.Append("2");
                            break;
                        default:
                            return string.Empty;
                    }
                }
                result.Append($"|{IPAddress}|{Port}|");
            }
            else
            {
                result
                    .Append(IPAddress?.ToString().Replace('.', ','))
                    .Append(',')
                    .Append(Port / 256)
                    .Append(',')
                    .Append(Port & 0xFF);
            }
            return result.ToString();
        }

        private static bool IsEnhancedAddress(string address)
        {
            var number = "0123456789";
            return number.IndexOf(address[0]) == -1;
        }

        private static Address? ParseLegacy(string address)
        {
            var addressParts = address.Split(',');
            if (addressParts.Length != 6)
            {
                return null;
            }

            var portHi = Convert.ToInt32(addressParts[4], 10);
            var portLo = Convert.ToInt32(addressParts[5], 10);
            var port = (portHi * 256) + portLo;
            var ipAddress = string.Join(".", addressParts, 0, 4);
            return new Address(ipAddress, port);
        }

        private static Address? ParseEnhanced(string address)
        {
            var dividerChar = address[0];
            var addressParts = address.Substring(1, address.Length - 2).Split(dividerChar);
            if (addressParts.Length != 3)
            {
                return null;
            }

            var port = Convert.ToInt32(addressParts[2], 10);
            var ipAddress = addressParts[1];

            if (string.IsNullOrEmpty(ipAddress))
            {
                return new Address(port);
            }

            int addressType;
            if (string.IsNullOrEmpty(addressParts[0]))
            {
                addressType = ipAddress.Contains(":") ? 2 : 1;
            }
            else
            {
                addressType = Convert.ToInt32(addressParts[0], 10);
            }

            switch (addressType)
            {
                case 1:
                case 2:
                    return new Address(ipAddress, port);
                default:
                    throw new NotSupportedException($"Unknown network protocol {addressType}");
            }
        }
    }
}
