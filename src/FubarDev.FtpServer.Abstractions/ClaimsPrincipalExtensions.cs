// <copyright file="ClaimsPrincipalExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Checks if the principal represents an anonymous user.
        /// </summary>
        /// <param name="principal">The principal to check.</param>
        /// <returns><see langword="true"/> when the principal contains an anonymous identity.</returns>
        public static bool IsAnonymous(this ClaimsPrincipal principal)
        {
            return principal.Identities.Any(x => x.IsAnonymous());
        }

        /// <summary>
        /// Checks if the principal represents an unix user.
        /// </summary>
        /// <param name="principal">The principal to check.</param>
        /// <returns><see langword="true"/> when the principal contains an unix identity.</returns>
        public static bool IsUnixUser(this ClaimsPrincipal principal)
        {
            return !string.IsNullOrEmpty(principal.FindFirst(FtpClaimTypes.UserId)?.Value);
        }
    }
}
