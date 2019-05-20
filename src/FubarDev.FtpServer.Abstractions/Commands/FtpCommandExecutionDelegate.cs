// <copyright file="FtpCommandExecutionDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// The delegate for the next middleware for the current FTP command execution step.
    /// </summary>
    /// <param name="context">The context of the current FTP command.</param>
    /// <returns>The task.</returns>
    [NotNull]
    public delegate Task FtpCommandExecutionDelegate(
        [NotNull] FtpExecutionContext context);
}
