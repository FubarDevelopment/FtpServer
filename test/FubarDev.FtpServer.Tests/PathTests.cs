// <copyright file="PathTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.DotNet.PlatformAbstractions;

using Xunit;

namespace FubarDev.FtpServer.Tests
{
    public class PathTests
    {
        [SkippableTheory]
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
        public void TestRootRemovalWindows(string expected, string input)
        {
            Skip.If(RuntimeEnvironment.OperatingSystemPlatform != Platform.Windows, "Works only on Windows");
            Assert.Equal(expected, input.RemoveRoot());
        }

        [SkippableTheory]
        [InlineData(null, null)]
        [InlineData(null, "")]
        [InlineData("a", "a")]
        [InlineData("a", "/a")]
        [InlineData("a", "//a")]
        public void TestRootRemovalNonWindows(string expected, string input)
        {
            Skip.If(RuntimeEnvironment.OperatingSystemPlatform == Platform.Windows, "Works only on non-Windows platforms");
            Assert.Equal(expected, input.RemoveRoot());
        }
    }
}
