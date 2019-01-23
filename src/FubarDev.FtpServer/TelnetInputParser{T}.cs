// <copyright file="TelnetInputParser{T}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Filters the TELNET commands usually sent before an ABOR command.
    /// </summary>
    /// <typeparam name="T">The return type of a <see cref="Collect"/> operation.</typeparam>
    public abstract class TelnetInputParser<T>
    {
        private bool _interpretAsCommandReceived;

        /// <summary>
        /// Collects data and handles the <c>Synch</c> and <c>Interrupt Process</c> TELNET commands.
        /// </summary>
        /// <param name="data">The data buffer.</param>
        /// <returns>The list of items found inside the collected data.</returns>
        public IReadOnlyList<T> Collect(ReadOnlySpan<byte> data)
        {
            var result = new List<T>();

            var dataOffset = 0;
            for (var index = 0; index != data.Length; ++index)
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
                            result.AddRange(DataReceived(data.Slice(index, 1)));
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
                        result.AddRange(DataReceived(data.Slice(dataOffset, dataLength)));
                    }

                    _interpretAsCommandReceived = true;
                    dataOffset = index + 2;
                }
            }

            if (dataOffset < data.Length)
            {
                var dataLength = data.Length - dataOffset;
                if (dataLength != 0)
                {
                    result.AddRange(DataReceived(data.Slice(dataOffset, dataLength)));
                }
            }

            return result;
        }

        /// <summary>
        /// Collects all non-TELNET data.
        /// </summary>
        /// <param name="data">The data buffer.</param>
        /// <returns>The collected items.</returns>
        protected abstract IEnumerable<T> DataReceived(ReadOnlySpan<byte> data);

        /// <summary>
        /// Handles the <c>Synch</c> command.
        /// </summary>
        /// <returns>The collected items.</returns>
        protected virtual IEnumerable<T> Synch()
        {
            Debug.WriteLine("TELNET: Synch command received");
            return new T[0];
        }

        /// <summary>
        /// Handles the <c>Interrupt Process</c> command.
        /// </summary>
        /// <returns>The collected items.</returns>
        protected virtual IEnumerable<T> InterruptProcess()
        {
            Debug.WriteLine("TELNET: Interrupt Process command received");
            return new T[0];
        }
    }
}
