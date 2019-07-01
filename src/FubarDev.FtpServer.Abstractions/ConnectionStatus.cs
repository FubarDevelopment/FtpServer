// <copyright file="ConnectionStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The status of the current connection.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// The initial status.
        /// </summary>
        Begin,

        /// <summary>
        /// Executing login.
        /// </summary>
        Login,

        /// <summary>
        /// User is authorized.
        /// </summary>
        Authorized,
    }
}
