// <copyright file="ShellStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using TestFtpServer.FtpServerShell;

namespace TestFtpServer
{
    /// <summary>
    /// Status for the FTP shell.
    /// </summary>
    public class ShellStatus : IShellStatus
    {
        /// <inheritdoc />
        public bool Closed { get; set; }
    }
}
