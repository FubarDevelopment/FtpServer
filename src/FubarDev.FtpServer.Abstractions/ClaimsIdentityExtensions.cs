// <copyright file="ClaimsIdentityExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="ClaimsIdentity"/>.
    /// </summary>
    public static class ClaimsIdentityExtensions
    {
        /// <summary>
        /// Checks if the identity represents an anonymous user.
        /// </summary>
        /// <param name="identity">The identity to check.</param>
        /// <returns><see langword="true"/> when identity is an anonymous user.</returns>
        public static bool IsAnonymous(this ClaimsIdentity identity)
        {
            return identity.HasClaim(c => c.Type == ClaimTypes.Anonymous);
        }
    }
}
