// <copyright file="IFtpMiddleware.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

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
        Task InvokeAsync(FtpContext context, FtpRequestDelegate next);
    }
}
