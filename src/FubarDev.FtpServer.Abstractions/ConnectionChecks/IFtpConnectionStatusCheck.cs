// <copyright file="IFtpConnectionStatusCheck.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionChecks
{
    /// <summary>
    /// Interface to check if the connection is still alive and kicking.
    /// </summary>
    public interface IFtpConnectionStatusCheck
    {
        /// <summary>
        /// Check if the connection is still alive.
        /// </summary>
        /// <returns><see langword="true"/> when the connection is still alive.</returns>
        bool CheckIfAlive();
    }
}
