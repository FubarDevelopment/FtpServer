// <copyright file="FtpClaimTypes.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Claim types for the FTP server.
    /// </summary>
    public static class FtpClaimTypes
    {
        /// <summary>
        /// The URI for a claim that specifies the users home path.
        /// </summary>
        public static readonly string HomePath = "http://schemas.fubar-dev.com/ws/2019/07/identity/claims/homepath";

        /// <summary>
        /// The URI for a claim that specifies the users Unix ID.
        /// </summary>
        public static readonly string UserId = "http://schemas.fubar-dev.com/ws/2019/07/identity/claims/user-id";

        /// <summary>
        /// The URI for a claim that specifies the users Unix main group ID.
        /// </summary>
        public static readonly string GroupId = "http://schemas.fubar-dev.com/ws/2019/07/identity/claims/group-id";
    }
}
