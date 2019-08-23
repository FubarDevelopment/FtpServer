// <copyright file="ClaimsPrincipalExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

namespace FubarDev.FtpServer.AccountManagement.Compatibility
{
    /// <summary>
    /// Extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    [Obsolete("Will only be used for compatibility purposes.")]
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Creates a <see cref="IFtpUser"/> for a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The claims principal to create the <see cref="IFtpUser"/> from.</param>
        /// <returns>The new object.</returns>
        [Obsolete("Will only be used for compatibility purposes.")]
        public static IFtpUser CreateUser(this ClaimsPrincipal principal)
        {
            if (principal.IsAnonymous())
            {
                return new AnonymousClaimsFtpUser(principal);
            }

            if (principal.IsUnixUser())
            {
                return new UnixClaimsFtpUser(principal);
            }

            return new ClaimsFtpUser(principal);
        }
    }
}
