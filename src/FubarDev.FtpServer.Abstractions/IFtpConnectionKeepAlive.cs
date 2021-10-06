// <copyright file="IFtpConnectionKeepAlive.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface to ensure that a connection keeps alive.
    /// </summary>
    [Obsolete("Use the IFtpConnectionStatusCheck feature instead.")]
    public interface IFtpConnectionKeepAlive
    {
        /// <summary>
        /// Gets a value indicating whether the connection is still alive.
        /// </summary>
        [Obsolete("Use the IFtpConnectionStatusCheck feature instead.")]
        bool IsAlive { get; }

        /// <summary>
        /// Gets the time of last activity (UTC).
        /// </summary>
        [Obsolete("This has never been useful.")]
        DateTime LastActivityUtc { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a data transfer is active.
        /// </summary>
        [Obsolete("Use the IFtpConnectionEventHost feature to publish events.")]
        bool IsInDataTransfer { get; set; }

        /// <summary>
        /// Ensure that the connection keeps alive.
        /// </summary>
        [Obsolete("Use the IFtpConnectionEventHost feature to publish events.")]
        void KeepAlive();
    }
}
