// <copyright file="IFtpMiddleware.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP middleware.
    /// </summary>
    public interface IFtpMiddleware
    {
        /// <summary>
        /// Function that gets invoked for the middleware.
        /// </summary>
        /// <param name="context">The context for the current FTP command.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>The task.</returns>
        [NotNull]
        Task InvokeAsync([NotNull] FtpContext context, [NotNull] FtpRequestDelegate next);
    }
}
