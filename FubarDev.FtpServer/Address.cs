//-----------------------------------------------------------------------
// <copyright file="Address.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Text;

namespace FubarDev.FtpServer
{
    public class Address
    {
        private readonly bool _isEnhanced;

        public Address(AddressFamily addressFamily, string address, int port)
        {
            _isEnhanced = true;
            AddressFamily = addressFamily;
            IpAddress = address;
            IpPort = port;
        }

        public Address(string address, int port)
        {
            _isEnhanced = false;
            AddressFamily = FubarDev.FtpServer.AddressFamily.IPv4;
            IpAddress = address;
            IpPort = port;
        }

        public Address(int port)
        {
            _isEnhanced = true;
            AddressFamily = FubarDev.FtpServer.AddressFamily.IPv4;
            IpAddress = null;
            IpPort = port;
        }

        public AddressFamily? AddressFamily { get; }

        public string IpAddress { get; }

        public int IpPort { get; }

        public static Address Parse(string address)
        {
            if (string.IsNullOrEmpty(address))
                return null;
            return IsEnhancedAddress(address)
                       ? ParseEnhanced(address)
                       : ParseLegacy(address);
        }

        public Uri ToUri()
        {
            if (AddressFamily != null && AddressFamily == FubarDev.FtpServer.AddressFamily.IPv6)
            {
                return new Uri($"port://[{IpAddress}]:{IpPort}/");
            }
            return new Uri($"port://{IpAddress}:{IpPort}/");
        }

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
                        case FubarDev.FtpServer.AddressFamily.IPv4:
                            result.Append("1");
                            break;
                        case FubarDev.FtpServer.AddressFamily.IPv6:
                            result.Append("2");
                            break;
                        default:
                            return string.Empty;
                    }
                }
                result.Append($"|{IpAddress}|{IpPort}|");
            }
            else
            {
                result
                    .Append(IpAddress.Replace('.', ','))
                    .Append(',')
                    .Append(IpPort / 256)
                    .Append(',')
                    .Append(IpPort & 0xFF);
            }
            return result.ToString();
        }

        private static bool IsEnhancedAddress(string address)
        {
            var number = "0123456789";
            return number.IndexOf(address[0]) == -1;
        }

        private static Address ParseLegacy(string address)
        {
            var addressParts = address.Split(',');
            if (addressParts.Length != 6)
                return null;
            var port = (Convert.ToInt32(addressParts[4], 10) * 256) + Convert.ToInt32(addressParts[5], 10);
            var ipAddress = string.Join(".", addressParts, 0, 4);
            return new Address(ipAddress, port);
        }

        private static Address ParseEnhanced(string address)
        {
            var dividerChar = address[0];
            var addressParts = address.Substring(1, address.Length - 2).Split(dividerChar);
            if (addressParts.Length != 3)
                return null;
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
                    return new Address(FubarDev.FtpServer.AddressFamily.IPv4, ipAddress, port);
                case 2:
                    return new Address(FubarDev.FtpServer.AddressFamily.IPv6, ipAddress, port);
                default:
                    throw new NotSupportedException($"Unknown network protocol {addressType}");
            }
        }
    }
}
