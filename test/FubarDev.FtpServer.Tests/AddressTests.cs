// <copyright file="AddressTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using Xunit;

namespace FubarDev.FtpServer.Tests
{
    [Obsolete]
    public class AddressTests
    {
        [Fact]
        public void TestEmpty()
        {
            var address = Address.Parse(string.Empty);
            Assert.Null(address);
            address = Address.Parse(null);
            Assert.Null(address);
        }

        [Fact]
        public void TestIpV4()
        {
            var address = Address.Parse("|1|132.235.1.2|6275|");
            Assert.NotNull(address);
            Assert.Equal(new Address("132.235.1.2", 6275), address, new AddressComparer() !);
            Assert.Equal(new Uri("port://132.235.1.2:6275"), address!.ToUri());
        }

        [Fact]
        public void TestLegacyIpV4()
        {
            var address = Address.Parse("132,235,1,2,24,131");
            Assert.NotNull(address);
            Assert.Equal(new Address("132.235.1.2", 6275), address, new AddressComparer() !);
            Assert.Equal(new Uri("port://132.235.1.2:6275"), address!.ToUri());
        }

        [Fact]
        public void TestIpV6()
        {
            var address = Address.Parse("|2|1080::8:800:200C:417A|5282|");
            Assert.NotNull(address);
            Assert.Equal(new Address("1080::8:800:200C:417A", 5282), address, new AddressComparer() !);
            Assert.Equal(new Uri("port://[1080::8:800:200C:417A]:5282"), address!.ToUri());
        }

        [Fact]
        public void TestPortOnly()
        {
            var address = Address.Parse("|||1234|");
            Assert.NotNull(address);
            Assert.Equal(new Address(1234), address, new AddressComparer() !);
            Assert.Throws<UriFormatException>(() => address!.ToUri());
        }

        private class AddressComparer : IEqualityComparer<Address>
        {
            public bool Equals(Address x, Address y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                if (x.AddressFamily == null)
                {
                    if (y.AddressFamily != null)
                    {
                        return false;
                    }
                }
                else if (y.AddressFamily == null)
                {
                    return false;
                }
                else if (x.AddressFamily != y.AddressFamily)
                {
                    return false;
                }

                return (x.IPAddress?.ToString() ?? string.Empty) == (y.IPAddress?.ToString() ?? string.Empty)
                       && x.Port == y.Port;
            }

            public int GetHashCode(Address obj)
            {
                return (obj.AddressFamily?.GetHashCode() ?? 0)
                       ^ (obj.IPAddress?.GetHashCode() ?? 0)
                       ^ obj.Port.GetHashCode();
            }
        }
    }
}
