// <copyright file="FtpConnectionAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Accessor for the active FTP connection
    /// </summary>
    public class FtpConnectionAccessor : IFtpConnectionAccessor
    {
        /// <inheritdoc />
        public IFtpConnection FtpConnection { get; set; }
    }
}
