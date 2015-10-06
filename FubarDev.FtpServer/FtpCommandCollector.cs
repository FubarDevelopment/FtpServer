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

namespace FubarDev.FtpServer
{
    public sealed class FtpCommandCollector : IDisposable
    {
        private static readonly char[] _whiteSpaces = { ' ', '\t' };

        private readonly Encoding _encoding;

        private MemoryStream _buffer = new MemoryStream();

        private bool _skipLineFeed;

        public FtpCommandCollector(Encoding encoding)
        {
            _encoding = encoding;
        }

        public Encoding Encoding => _encoding;

        public byte[] ToArray()
        {
            return _buffer.ToArray();
        }

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

        public void Dispose()
        {
            if (_buffer != null)
                _buffer.Dispose();
            _buffer = null;
        }

        private FtpCommand CreateFtpCommand(byte[] command)
        {
            var message = _encoding.GetString(command, 0, command.Length);
            var spaceIndex = message.IndexOfAny(_whiteSpaces);
            var commandName = spaceIndex == -1 ? message : message.Substring(0, spaceIndex);
            var commandArguments = spaceIndex == -1 ? string.Empty : message.Substring(spaceIndex + 1);
            return new FtpCommand(commandName, commandArguments);
        }
    }
}
