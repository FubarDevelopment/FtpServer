//-----------------------------------------------------------------------
// <copyright file="MemberValidationStatus.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Validation result for a <see cref="IMembershipProvider"/>.
    /// </summary>
    public enum MemberValidationStatus
    {
        /// <summary>
        /// User name or password invalid.
        /// </summary>
        InvalidLogin,

        /// <summary>
        /// Email address validation for anonymous login failed.
        /// </summary>
        InvalidAnonymousEmail,

        /// <summary>
        /// Anonymous user.
        /// </summary>
        Anonymous,

        /// <summary>
        /// User authenticated successfully.
        /// </summary>
        AuthenticatedUser,
    }
}
