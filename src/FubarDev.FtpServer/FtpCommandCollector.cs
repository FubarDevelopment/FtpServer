//-----------------------------------------------------------------------
// <copyright file="FtpCommandCollector.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Collects FTP commands using the current <see cref="System.Text.Encoding"/>.
    /// </summary>
    public sealed class FtpCommandCollector
    {
        private readonly Func<Encoding> _getActiveEncodingFunc;

        private readonly FtpTelnetInputParser _telnetInputParser;

        private readonly List<byte[]> _buffer = new List<byte[]>();

        private bool _skipLineFeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandCollector"/> class.
        /// </summary>
        /// <param name="getActiveEncodingFunc">The delegate to get the current encoding for.</param>
        public FtpCommandCollector(Func<Encoding> getActiveEncodingFunc)
        {
            _telnetInputParser = new FtpTelnetInputParser(this);
            _getActiveEncodingFunc = getActiveEncodingFunc;
        }

        /// <summary>
        /// Gets the currently active <see cref="System.Text.Encoding"/>.
        /// </summary>
        public Encoding Encoding => _getActiveEncodingFunc();

        /// <summary>
        /// Gets a value indicating whether this collector contains unused data.
        /// </summary>
        public bool IsEmpty => _buffer.Count == 0;

        /// <summary>
        /// Collects the data from the <paramref name="buffer"/> and tries to build <see cref="FtpCommand"/> objects from it.
        /// </summary>
        /// <param name="buffer">The buffer to collect the data from.</param>
        /// <returns>The found <see cref="FtpCommand"/>s.</returns>
        [NotNull]
        [ItemNotNull]
        public IEnumerable<FtpCommand> Collect(ReadOnlySpan<byte> buffer)
        {
#if DEBUG
            var collectedData = new StringBuilder();
            for (var i = 0; i < buffer.Length; i++)
            {
                collectedData.Append(buffer[i].ToString("X2"));
            }

            Debug.WriteLine("Collected data: {0}", collectedData);
#endif

            var commands = new List<FtpCommand>();
            commands.AddRange(_telnetInputParser.Collect(buffer));
            return commands;
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<FtpCommand> InternalCollect(ReadOnlySpan<byte> buffer)
        {
            var commands = new List<FtpCommand>();

            if (buffer.Length == 0)
            {
                return commands;
            }

            if (_skipLineFeed && buffer[0] == '\n')
            {
                buffer = buffer.Slice(1);
            }

            _skipLineFeed = false;

            do
            {
                var carriageReturnPos = buffer.IndexOf((byte)'\r');
                if (carriageReturnPos == -1)
                {
                    break;
                }

                _skipLineFeed = true;
                if (carriageReturnPos != 0)
                {
                    // Store the found data into the buffer
                    _buffer.Add(buffer.Slice(0, carriageReturnPos).ToArray());
                }

                if (_buffer.Count != 0)
                {
                    // We've got a non-empty line
                    var bufferLength = _buffer.Sum(x => x.Length);
                    var data = new byte[bufferLength];
                    var dataIndex = 0;
                    foreach (var item in _buffer)
                    {
                        Array.Copy(item, 0, data, dataIndex, item.Length);
                        dataIndex += item.Length;
                    }

                    _buffer.Clear();

                    commands.Add(CreateFtpCommand(data));
                }

                if (carriageReturnPos < buffer.Length - 1)
                {
                    if (buffer[carriageReturnPos + 1] == '\n')
                    {
                        buffer = buffer.Slice(carriageReturnPos + 2);
                        _skipLineFeed = false;
                    }
                    else
                    {
                        buffer = buffer.Slice(carriageReturnPos + 1);
                    }
                }
                else
                {
                    buffer = ReadOnlySpan<byte>.Empty;
                }
            }
            while (buffer.Length != 0);

            if (buffer.Length != 0)
            {
                _buffer.Add(buffer.ToArray());
            }

            return commands;
        }

        private FtpCommand CreateFtpCommand([NotNull] byte[] command)
        {
            var message = Encoding.GetString(command, 0, command.Length);
            return FtpCommand.Parse(message);
        }

        private class FtpTelnetInputParser : TelnetInputParser<FtpCommand>
        {
            private readonly FtpCommandCollector _collector;

            public FtpTelnetInputParser(FtpCommandCollector collector)
            {
                _collector = collector;
            }

            protected override IEnumerable<FtpCommand> DataReceived(ReadOnlySpan<byte> data)
            {
                return _collector.InternalCollect(data);
            }
        }
    }
}
