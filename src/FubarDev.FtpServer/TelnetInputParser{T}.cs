// <copyright file="TelnetInputParser{T}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Filters the TELNET commands usually sent before an ABOR command.
    /// </summary>
    /// <typeparam name="T">The return type of a <see cref="Collect(byte[], int, int)"/> operation.</typeparam>
    public abstract class TelnetInputParser<T>
    {
        private bool _interpretAsCommandReceived;

        /// <summary>
        /// Collects data and handles the <code>Synch</code> and <code>Interrupt Process</code> TELNET commands.
        /// </summary>
        /// <param name="data">The data buffer.</param>
        /// <param name="offset">The offset into the data buffer.</param>
        /// <param name="length">The length of the data to read from the data buffer.</param>
        /// <returns>The list of items found inside the collected data.</returns>
        public IReadOnlyList<T> Collect(byte[] data, int offset, int length)
        {
            var result = new List<T>();
            var endOffset = offset + length;
            var dataOffset = offset;
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
                            result.AddRange(Synch());
                            break;
                        case 0xF4:
                            result.AddRange(InterruptProcess());
                            break;
                        case 0xFF:
                            // Double-Escape
                            result.AddRange(DataReceived(data, index, length: 1));
                            break;
                        default:
                            Debug.WriteLine("TELNET: Unknown command received - skipping 0xFF");
                            dataOffset = index;
                            break;
                    }
                    _interpretAsCommandReceived = false;
                }
                else if (v == 0xFF)
                {
                    var dataLength = index - dataOffset;
                    if (dataLength != 0)
                    {
                        result.AddRange(DataReceived(data, dataOffset, dataLength));
                    }

                    _interpretAsCommandReceived = true;
                    dataOffset = index + 2;
                }
            }
            if (dataOffset < endOffset)
            {
                var dataLength = endOffset - dataOffset;
                if (dataLength != 0)
                {
                    result.AddRange(DataReceived(data, dataOffset, dataLength));
                }
            }
            return result;
        }

        /// <summary>
        /// Collects all non-TELNET data.
        /// </summary>
        /// <param name="data">The data buffer.</param>
        /// <param name="offset">The offset into the data buffer.</param>
        /// <param name="length">The length of the data to be collected.</param>
        /// <returns>The collected items.</returns>
        protected abstract IEnumerable<T> DataReceived(byte[] data, int offset, int length);

        /// <summary>
        /// Handles the <code>Synch</code> command.
        /// </summary>
        /// <returns>The collected items.</returns>
        protected virtual IEnumerable<T> Synch()
        {
            Debug.WriteLine("TELNET: Synch command received");
            return new T[0];
        }

        /// <summary>
        /// Handles the <code>Interrupt Process</code> command.
        /// </summary>
        /// <returns>The collected items.</returns>
        protected virtual IEnumerable<T> InterruptProcess()
        {
            Debug.WriteLine("TELNET: Interrupt Process command received");
            return new T[0];
        }
    }
}
