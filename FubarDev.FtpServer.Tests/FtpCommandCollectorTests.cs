using System;
using System.Collections.Generic;
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
                new[] {
                    new FtpCommand("USER", "anonymous"),
                },
                commands,
                new FtpCommandComparer());
        }

        [Fact]
        public void TestCompleteCarriageReturnOnly()
        {
            var collector = new FtpCommandCollector(() => Encoding.UTF8);
            var commands = Collect(collector, "TEST\r");
            Assert.Equal(
                new[]
                {
                    new FtpCommand("TEST", string.Empty),
                },
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
                new[]
                {
                    new FtpCommand("TEST", string.Empty),
                },
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
                new[]
                {
                    new FtpCommand("TEST", string.Empty),
                },
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
                new[]
                {
                    new FtpCommand("TEST", string.Empty),
                },
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
                new[]
                {
                    new FtpCommand("TEST1", string.Empty),
                    new FtpCommand("TEST2", string.Empty),
                },
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
                new[]
                {
                    new FtpCommand("TEST1", string.Empty),
                },
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
                new[]
                {
                    new FtpCommand("TEST1", string.Empty),
                    new FtpCommand("TEST2", string.Empty),
                },
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
                new[]
                {
                    new FtpCommand("TEST", "1"),
                },
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
                new[]
                {
                    new FtpCommand("TEST", "1"),
                    new FtpCommand("TEST", "2"),
                },
                commands,
                new FtpCommandComparer());
            Assert.True(collector.IsEmpty);
        }

        private IEnumerable<FtpCommand> Collect(FtpCommandCollector collector, string data)
        {
            var temp = collector.Encoding.GetBytes(data);
            return collector.Collect(temp, 0, temp.Length);
        }

        private class FtpCommandComparer : IComparer<FtpCommand>, IEqualityComparer<FtpCommand>
        {
            private static readonly StringComparer StringComparer = StringComparer.OrdinalIgnoreCase;

            public int Compare(FtpCommand x, FtpCommand y)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                if (ReferenceEquals(x, null) && !ReferenceEquals(y, null))
                    return -1;
                if (ReferenceEquals(y, null))
                    return 1;
                var v = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                if (v != 0)
                    return v;
                return StringComparer.Compare(x.Argument ?? string.Empty, y.Argument ?? string.Empty);
            }

            public bool Equals(FtpCommand x, FtpCommand y)
            {
                return Compare(x, y) == 0;
            }

            public int GetHashCode(FtpCommand obj)
            {
                return StringComparer.GetHashCode(obj.Name)
                       ^ StringComparer.GetHashCode(obj.Argument ?? string.Empty);
            }
        }
    }
}
