//-----------------------------------------------------------------------
// <copyright file="FtpCommandCollector.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Collects FTP commands using the current <see cref="System.Text.Encoding"/>
    /// </summary>
    public sealed class FtpCommandCollector : IDisposable
    {
        private static readonly char[] _whiteSpaces = { ' ', '\t' };

        private readonly Func<Encoding> _getActiveEncodingFunc;

        private MemoryStream _buffer = new MemoryStream();

        private bool _skipLineFeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandCollector"/> class.
        /// </summary>
        /// <param name="getActiveEncodingFunc">The delegate to get the current encoding for</param>
        public FtpCommandCollector(Func<Encoding> getActiveEncodingFunc)
        {
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
        [NotNull, ItemNotNull]
        public IEnumerable<FtpCommand> Collect(byte[] buffer, int offset, int length)
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

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_buffer != null)
                _buffer.Dispose();
            _buffer = null;
        }

        private FtpCommand CreateFtpCommand(byte[] command)
        {
            var message = Encoding.GetString(command, 0, command.Length);
            var spaceIndex = message.IndexOfAny(_whiteSpaces);
            var commandName = spaceIndex == -1 ? message : message.Substring(0, spaceIndex);
            var commandArguments = spaceIndex == -1 ? string.Empty : message.Substring(spaceIndex + 1);
            return new FtpCommand(commandName, commandArguments);
        }
    }
}
