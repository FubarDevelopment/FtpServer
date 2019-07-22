// <copyright file="PortStringExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Sockets;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for PORT/EPRT arguments.
    /// </summary>
    public static class PortStringExtensions
    {
        /// <summary>
        /// Parses an IP address.
        /// </summary>
        /// <param name="address">The IP address to parse.</param>
        /// <param name="remoteEndPoint">The remote end point of the connection.</param>
        /// <returns>The parsed IP address.</returns>
        public static IPEndPoint? ParsePortString(this string address, IPEndPoint remoteEndPoint)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            return IsEnhancedAddress(address)
                ? ParseEnhanced(address, remoteEndPoint)
                : ParseLegacy(address);
        }

        private static bool IsEnhancedAddress(string address)
        {
            var number = "0123456789";
            return number.IndexOf(address[0]) == -1;
        }

        private static IPEndPoint? ParseLegacy(string address)
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
            return new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        private static IPEndPoint? ParseEnhanced(string address, IPEndPoint remoteEndPoint)
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
                return new IPEndPoint(remoteEndPoint.Address, port);
            }

            return new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        private static AddressFamily GetAddressFamily(string addressFamilyNumber, string ipAddress)
        {
            int addressType;
            if (string.IsNullOrEmpty(addressFamilyNumber))
            {
                if (string.IsNullOrEmpty(ipAddress))
                {
                    addressType = -1;
                }
                else
                {
                    addressType = ipAddress.Contains(":") ? 2 : 1;
                }
            }
            else
            {
                addressType = Convert.ToInt32(addressFamilyNumber, 10);
            }

            AddressFamily af;
            switch (addressType)
            {
                case -1:
                    af = AddressFamily.Unknown;
                    break;
                case 1:
                    af = AddressFamily.InterNetwork;
                    break;
                case 2:
                    af = AddressFamily.InterNetworkV6;
                    break;
                default:
                    throw new NotSupportedException($"Unknown network protocol {addressType}");
            }

            return af;
        }
    }
}
