// <copyright file="IFtpConnectionKeepAlive.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface to ensure that a connection keeps alive.
    /// </summary>
    public interface IFtpConnectionKeepAlive
    {
        /// <summary>
        /// Gets a value indicating whether the connection is still alive.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Gets the time of last activity (UTC).
        /// </summary>
        DateTime LastActivityUtc { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a data transfer is active.
        /// </summary>
        bool IsInDataTransfer { get; set; }

        /// <summary>
        /// Ensure that the connection keeps alive.
        /// </summary>
        void KeepAlive();
    }
}
