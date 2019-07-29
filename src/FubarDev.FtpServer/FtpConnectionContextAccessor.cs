// <copyright file="FtpConnectionContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Accessor for the active FTP connection.
    /// </summary>
    public class FtpConnectionContextAccessor : IFtpConnectionContextAccessor
    {
        private static readonly AsyncLocal<FtpConnectionContextHolder> _ftpConnectionContextCurrent = new AsyncLocal<FtpConnectionContextHolder>();

        /// <inheritdoc />
        public IFtpConnectionContext FtpConnectionContext
        {
            get
            {
                return _ftpConnectionContextCurrent.Value?.ConnectionContext ?? throw new InvalidOperationException("No active connection");
            }
            set
            {
                var holder = _ftpConnectionContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current IFtpConnection trapped in the AsyncLocals, as its done.
                    holder.ConnectionContext = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the IFtpConnection in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _ftpConnectionContextCurrent.Value = new FtpConnectionContextHolder()
                    {
                        ConnectionContext = value,
                    };
                }
            }
        }

        private class FtpConnectionContextHolder
        {
            public IFtpConnectionContext? ConnectionContext { get; set; }
        }
    }
}
