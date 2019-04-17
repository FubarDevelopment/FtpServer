// <copyright file="SecurityStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The states of the FTP login state machine.
    /// </summary>
    public enum SecurityStatus
    {
        /// <summary>
        /// Unauthenticated, either <c>AUTH</c> or <c>USER</c> is needed for authentication/authorization.
        /// </summary>
        Unauthenticated,

        /// <summary>
        /// The <c>AUTH</c> command needs authentication data (<c>ADAT</c>).
        /// </summary>
        NeedSecurityData,

        /// <summary>
        /// The user is authenticated. Authorization is needed.
        /// </summary>
        Authenticated,

        /// <summary>
        /// User needs to issue a password.
        /// </summary>
        NeedPassword,

        /// <summary>
        /// An additional <c>ACCT</c> command is required to complete the authorization.
        /// </summary>
        NeedAccount,

        /// <summary>
        /// The user is authorized.
        /// </summary>
        Authorized,
    }
}
