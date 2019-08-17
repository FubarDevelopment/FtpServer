// <copyright file="IFtpCommandMiddleware.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// FTP command middleware.
    /// </summary>
    public interface IFtpCommandMiddleware
    {
        /// <summary>
        /// Function that gets invoked for the middleware.
        /// </summary>
        /// <param name="context">The context for the current FTP command.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>The task.</returns>
        Task InvokeAsync(
            FtpExecutionContext context,
            FtpCommandExecutionDelegate next);
    }
}
