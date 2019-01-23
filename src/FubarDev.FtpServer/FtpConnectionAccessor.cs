// <copyright file="FtpConnectionAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Accessor for the active FTP connection.
    /// </summary>
    public class FtpConnectionAccessor : IFtpConnectionAccessor
    {
        private static readonly AsyncLocal<FtpConnectionHolder> _ftpConnectionCurrent = new AsyncLocal<FtpConnectionHolder>();

        /// <inheritdoc />
        public IFtpConnection FtpConnection
        {
            get
            {
                return _ftpConnectionCurrent.Value?.Connection;
            }
            set
            {
                var holder = _ftpConnectionCurrent.Value;
                if (holder != null)
                {
                    // Clear current IFtpConnection trapped in the AsyncLocals, as its done.
                    holder.Connection = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the IFtpConnection in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _ftpConnectionCurrent.Value = new FtpConnectionHolder()
                    {
                        Connection = value,
                    };
                }
            }
        }

        private class FtpConnectionHolder
        {
            public IFtpConnection Connection { get; set; }
        }
    }
}
