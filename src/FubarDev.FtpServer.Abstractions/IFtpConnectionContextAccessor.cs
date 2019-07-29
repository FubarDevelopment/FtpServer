// <copyright file="IFtpConnectionContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Accessor to get/set the current (scoped) FTP connection.
    /// </summary>
    public interface IFtpConnectionContextAccessor
    {
        /// <summary>
        /// Gets or sets the current FTP connection.
        /// </summary>
        IFtpConnectionContext FtpConnectionContext { get; set; }
    }
}
