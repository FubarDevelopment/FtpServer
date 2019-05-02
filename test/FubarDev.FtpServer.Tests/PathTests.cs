// <copyright file="PathTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Xunit;

namespace FubarDev.FtpServer.Tests
{
    public class PathTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "")]
        [InlineData("a", "a")]
        [InlineData(null, @"a:\")]
        [InlineData(null, @"\")]
        [InlineData(null, "/")]
        [InlineData(null, @"\\")]
        [InlineData(null, @"\\.")]
        [InlineData(null, @"\\.\")]
        [InlineData(null, @"\\.\a")]
        [InlineData(null, @" \\.\a")]
        [InlineData(null, @"\\. \a")]
        [InlineData(null, @"\\.\ a")]
        [InlineData(" a", @"\\.\\ a")]
        [InlineData("a", @"\\.\\\a")]
        [InlineData("b", @"\\.\a\b")]
        [InlineData(" b", @"\\.\a\ b")]
        [InlineData(null, @"\\ .\a")]

        // Will fail under .NET Framework
        // [InlineData(@" \.\a", @"\ \.\a")]
        public void TestRootRemoval(string expected, string input)
        {
            Assert.Equal(expected, input.RemoveRoot());
        }
    }
}
