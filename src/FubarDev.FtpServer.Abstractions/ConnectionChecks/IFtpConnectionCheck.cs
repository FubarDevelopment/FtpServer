// <copyright file="IFtpConnectionCheck.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionChecks
{
    /// <summary>
    /// Check if the connection is usable/connected/not idle.
    /// </summary>
    public interface IFtpConnectionCheck
    {
        /// <summary>
        /// Check if the connection is usable.
        /// </summary>
        /// <param name="context">The FTP connection check context.</param>
        /// <returns><see cref="FtpConnectionCheckResult"/> that contains the result of the check.</returns>
        FtpConnectionCheckResult Check(FtpConnectionCheckContext context);
    }
}
