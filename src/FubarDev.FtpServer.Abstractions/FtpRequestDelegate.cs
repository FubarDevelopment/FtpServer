// <copyright file="FtpRequestDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The delegate for the next middleware for the current request.
    /// </summary>
    /// <param name="context">The context of the current FTP command.</param>
    /// <returns>The task.</returns>
    [NotNull]
    public delegate Task FtpRequestDelegate([NotNull] FtpCommandContext context);
}
