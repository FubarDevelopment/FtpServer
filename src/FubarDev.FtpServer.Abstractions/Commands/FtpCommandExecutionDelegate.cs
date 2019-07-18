// <copyright file="FtpCommandExecutionDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// The delegate for the next middleware for the current FTP command execution step.
    /// </summary>
    /// <param name="context">The context of the current FTP command.</param>
    /// <returns>The task.</returns>
    public delegate Task FtpCommandExecutionDelegate(FtpExecutionContext context);
}
