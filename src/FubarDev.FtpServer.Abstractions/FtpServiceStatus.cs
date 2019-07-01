// <copyright file="FtpServiceStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The current status of the <see cref="IPausableFtpService"/>.
    /// </summary>
    public enum FtpServiceStatus
    {
        /// <summary>
        /// The service is ready to run.
        /// </summary>
        ReadyToRun,

        /// <summary>
        /// The service was stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// The service was paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The service is running.
        /// </summary>
        Running,
    }
}
