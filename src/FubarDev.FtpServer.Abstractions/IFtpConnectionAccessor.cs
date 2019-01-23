// <copyright file="IFtpConnectionAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Accessor to get/set the current (scoped) FTP connection.
    /// </summary>
    public interface IFtpConnectionAccessor
    {
        /// <summary>
        /// Gets or sets the current FTP connection.
        /// </summary>
        IFtpConnection FtpConnection { get; set; }
    }
}
