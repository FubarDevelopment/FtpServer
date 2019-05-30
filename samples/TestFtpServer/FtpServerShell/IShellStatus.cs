// <copyright file="IShellStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace TestFtpServer.FtpServerShell
{
    /// <summary>
    /// Status of the FTP server shell.
    /// </summary>
    public interface IShellStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether the FTP server should be closed.
        /// </summary>
        bool Closed { get; set; }
    }
}
