// <copyright file="FtpCommandCollectorTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

namespace FubarDev.FtpServer.Tests
{
    public class FtpCommandCollectorTests
    {
        [Fact]
        public void TestIncomplete()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = Collect(collector, "TEST");
            Assert.Equal(new FtpCommand[0], commands);
            Assert.False(collector.IsEmpty);
        }

        [Fact]
        public void TestSingleChars()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            foreach (var ch in "USER anonymous\r\n")
            {
                commands.AddRange(Collect(collector, $"{ch}"));
            }

            Assert.Equal(
                new[] { new FtpCommand("USER", "anonymous") },
                commands,
                new FtpCommandComparer());
        }

        [Fact]
        public void TestCompleteCarriageReturnOnly()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = Collect(collector, "TEST\r");
            Assert.Equal(
                new[] { new FtpCommand("TEST", string.Empty) },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestCompleteCarriageReturnWithLineFeed()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = Collect(collector, "TEST\r\n");
            Assert.Equal(
                new[] { new FtpCommand("TEST", string.Empty) },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestCompleteCarriageReturnWithLineFeedAtStepTwo()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            commands.AddRange(Collect(collector, "TEST\r"));
            commands.AddRange(Collect(collector, "\n"));
            Assert.Equal(
                new[] { new FtpCommand("TEST", string.Empty), },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestCompleteInTwoSteps()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            commands.AddRange(Collect(collector, "TES"));
            commands.AddRange(Collect(collector, "T\r\n"));
            Assert.Equal(
                new[] { new FtpCommand("TEST", string.Empty), },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestMultipleWithoutLineFeed()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            commands.AddRange(Collect(collector, "TEST1\r"));
            commands.AddRange(Collect(collector, "TEST2\r\n"));
            Assert.Equal(
                new[] { new FtpCommand("TEST1", string.Empty), new FtpCommand("TEST2", string.Empty) },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestMultipleWithSecondIncomplete()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            commands.AddRange(Collect(collector, "TEST1\rTEST2"));
            Assert.Equal(
                new[] { new FtpCommand("TEST1", string.Empty) },
                commands,
                new FtpCommandComparer());
            Assert.False(collector.IsEmpty);
        }

        [Fact]
        public void TestMultipleWithLineFeedInStepTwo()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            commands.AddRange(Collect(collector, "TEST1\r"));
            commands.AddRange(Collect(collector, "\nTEST2\r\n"));
            Assert.Equal(
                new[] { new FtpCommand("TEST1", string.Empty), new FtpCommand("TEST2", string.Empty) },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestWithArgument()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            commands.AddRange(Collect(collector, "TEST 1\r"));
            Assert.Equal(
                new[] { new FtpCommand("TEST", "1") },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestMultipleWithArgumentWithLineFeedInStepTwo()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = new List<FtpCommand>();
            commands.AddRange(Collect(collector, "TEST 1\r"));
            commands.AddRange(Collect(collector, "\nTEST 2\r\n"));
            Assert.Equal(
                new[] { new FtpCommand("TEST", "1"), new FtpCommand("TEST", "2") },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        [Fact]
        public void TestWithCyrillicTextWithWindows1251Encoding()
        {
            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(codepage: 1251);
            var collector = new FtpCommandCollector(() => encoding);

            const string cyrillicSymbols = "абвгдеёжзийклмнопрстуфхцчшщыъьэюя";

            var expectedCommands = new[] { new FtpCommand("TEST", cyrillicSymbols) };

            var stringToTest = string.Format("TEST {0}\r\n", cyrillicSymbols);
            var actualCommands = Collect(collector, stringToTest).ToArray();

            // Test failed
            // TELNET: Unknown command received - skipping 0xFF
            // 0xFF == 'я' (maybe?)
            Assert.Equal(expectedCommands, actualCommands, new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        private static IEnumerable<ReadOnlyMemory<byte>> EscapeIAC(byte[] data)
        {
            var startIndex = 0;
            for (var i = 0; i != data.Length; ++i)
            {
                if (data[i] == 0xFF)
                {
                    var length = i - startIndex + 1;
                    yield return new ReadOnlyMemory<byte>(data, startIndex, length);
                    startIndex = i;
                }
            }

            var remaining = data.Length - startIndex;
            if (remaining != 0)
            {
                yield return new ReadOnlyMemory<byte>(data, startIndex, remaining);
            }
        }

        private static IEnumerable<FtpCommand> Collect(FtpCommandCollector collector, string data)
        {
            var temp = collector.Encoding.GetBytes(data);
            foreach (var escapedDataMemory in EscapeIAC(temp))
            {
                var collected = collector.Collect(escapedDataMemory.Span);
                foreach (var command in collected)
                {
                    yield return command;
                }
            }
        }

        private class FtpCommandComparer : IComparer<FtpCommand>, IEqualityComparer<FtpCommand>
        {
            private static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

            public int Compare(FtpCommand x, FtpCommand y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                var v = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                if (v != 0)
                {
                    return v;
                }

                return _stringComparer.Compare(x.Argument, y.Argument);
            }

            public bool Equals(FtpCommand x, FtpCommand y)
            {
                return Compare(x, y) == 0;
            }

            public int GetHashCode(FtpCommand obj)
            {
                return _stringComparer.GetHashCode(obj.Name)
                       ^ _stringComparer.GetHashCode(obj.Argument);
            }
        }
    }
}
