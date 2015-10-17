// <copyright file="TelnetCommandCollector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    public abstract class TelnetInputParser<T>
    {
        private bool _interpretAsCommandReceived;

        public IReadOnlyList<T> Collect(byte[] data, int offset, int length)
        {
            var result = new List<T>();
            var endOffset = offset + length;
            int dataOffset = offset;
            for (var index = offset; index != endOffset; ++index)
            {
                var v = data[index];
                if (_interpretAsCommandReceived)
                {
                    dataOffset = index + 1;
                    _interpretAsCommandReceived = false;
                    switch (v)
                    {
                        case 0xF2:
                            result.AddRange(Sync());
                            break;
                        case 0xF4:
                            result.AddRange(InterruptProcess());
                            break;
                        default:
                            dataOffset = offset;
                            break;
                    }
                }
                else if (v == 0xFF)
                {
                    var dataLength = index - dataOffset;
                    if (dataLength != 0)
                        result.AddRange(DataReceived(data, dataOffset, dataLength));
                    _interpretAsCommandReceived = true;
                    dataOffset = index + 2;
                }
            }
            if (dataOffset < endOffset)
            {
                var dataLength = endOffset - dataOffset;
                if (dataLength != 0)
                    result.AddRange(DataReceived(data, dataOffset, dataLength));
            }
            return result;
        }

        protected virtual IEnumerable<T> DataReceived(byte[] data, int offset, int length)
        {
            return new T[0];
        }

        protected virtual IEnumerable<T> Sync()
        {
            return new T[0];
        }

        protected virtual IEnumerable<T> InterruptProcess()
        {
            return new T[0];
        }
    }
}
