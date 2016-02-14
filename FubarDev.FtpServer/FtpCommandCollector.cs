//-----------------------------------------------------------------------
// <copyright file="FtpCommandCollector.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Collects FTP commands using the current <see cref="System.Text.Encoding"/>
    /// </summary>
    public sealed class FtpCommandCollector : IDisposable
    {
        private readonly Func<Encoding> _getActiveEncodingFunc;

        private readonly FtpTelnetInputParser _telnetInputParser;

        private MemoryStream _buffer = new MemoryStream();

        private bool _skipLineFeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandCollector"/> class.
        /// </summary>
        /// <param name="getActiveEncodingFunc">The delegate to get the current encoding for</param>
        public FtpCommandCollector(Func<Encoding> getActiveEncodingFunc)
        {
            _telnetInputParser = new FtpTelnetInputParser(this);
            _getActiveEncodingFunc = getActiveEncodingFunc;
        }

        /// <summary>
        /// Gets the currently active <see cref="System.Text.Encoding"/>
        /// </summary>
        public Encoding Encoding => _getActiveEncodingFunc();

        /// <summary>
        /// Gets a value indicating whether this collector contains unused data
        /// </summary>
        public bool IsEmpty => _buffer.Length == 0;

        /// <summary>
        /// Collects the data from the <paramref name="buffer"/> and tries to build <see cref="FtpCommand"/> objects from it.
        /// </summary>
        /// <param name="buffer">The buffer to collect the data from</param>
        /// <param name="offset">An offset into the buffer to collect the data from</param>
        /// <param name="length">The length of the data to collect</param>
        /// <returns>The found <see cref="FtpCommand"/>s</returns>
        [NotNull]
        [ItemNotNull]
        public IEnumerable<FtpCommand> Collect(byte[] buffer, int offset, int length)
        {
            Debug.WriteLine("Collected data: {0}", string.Join(string.Empty, Enumerable.Range(offset, length).Select(x => buffer[x].ToString("X2"))));

            var commands = new List<FtpCommand>();
            commands.AddRange(_telnetInputParser.Collect(buffer, offset, length));
            return commands;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _buffer?.Dispose();
            _buffer = null;
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<FtpCommand> InternalCollect(byte[] buffer, int offset, int length)
        {
            var commands = new List<FtpCommand>();

            var skipLineFeedOffset = _skipLineFeed && buffer[offset] == '\n' ? 1 : 0;
            offset += skipLineFeedOffset;
            length -= skipLineFeedOffset;
            _skipLineFeed = false;

            do
            {
                var carriageReturnPos = Array.IndexOf(buffer, (byte)'\r', offset, length);
                if (carriageReturnPos == -1)
                    break;

                _skipLineFeed = true;
                var previousData = _buffer.ToArray();
                var data = new byte[carriageReturnPos - offset + previousData.Length];
                if (previousData.Length != 0)
                    Array.Copy(previousData, data, previousData.Length);
                if ((carriageReturnPos - offset) != 0)
                    Array.Copy(buffer, offset, data, previousData.Length, carriageReturnPos - offset);

                commands.Add(CreateFtpCommand(data));

                var copyLength = carriageReturnPos - offset;
                offset += copyLength + 1;
                length -= copyLength + 1;

                if (length == 0)
                {
                    _buffer = new MemoryStream();
                    _skipLineFeed = true;
                    break;
                }

                if (buffer[offset] == '\n')
                {
                    var tempBuffer = new byte[length - 1];
                    Array.Copy(buffer, offset + 1, tempBuffer, 0, tempBuffer.Length);
                    buffer = tempBuffer;
                    length = tempBuffer.Length;
                    offset = 0;
                    if (length == 0)
                        _buffer = new MemoryStream();
                }
                else
                {
                    _skipLineFeed = true;
                }
            }
            while (length != 0);

            if (length != 0)
                _buffer.Write(buffer, offset, length);
            return commands;
        }

        private FtpCommand CreateFtpCommand(byte[] command)
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

            protected override IEnumerable<FtpCommand> DataReceived(byte[] data, int offset, int length)
            {
                return _collector.InternalCollect(data, offset, length);
            }
        }
    }
}
