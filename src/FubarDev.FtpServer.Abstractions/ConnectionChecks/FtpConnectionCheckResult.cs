// <copyright file="FtpConnectionCheckResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ConnectionChecks
{
    /// <summary>
    /// Result object for a connection usability check.
    /// </summary>
    public sealed class FtpConnectionCheckResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionCheckResult"/> class.
        /// </summary>
        /// <param name="isUsable">A value indicating whether the connection is usable.</param>
        public FtpConnectionCheckResult(bool isUsable)
        {
            IsUsable = isUsable;
        }

        /// <summary>
        /// Gets a value indicating whether the connection is usable.
        /// </summary>
        public bool IsUsable { get; }
    }
}
