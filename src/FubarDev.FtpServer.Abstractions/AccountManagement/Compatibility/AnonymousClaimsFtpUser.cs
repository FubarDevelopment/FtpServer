// <copyright file="AnonymousClaimsFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

namespace FubarDev.FtpServer.AccountManagement.Compatibility
{
    /// <summary>
    /// Implementation of <see cref="IAnonymousFtpUser"/> which uses <see cref="ClaimsPrincipal"/> under the hoods.
    /// </summary>
    [Obsolete("Will only be used for compatibility purposes.")]
    public class AnonymousClaimsFtpUser : ClaimsFtpUser, IAnonymousFtpUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousClaimsFtpUser"/> class.
        /// </summary>
        /// <param name="principal">The underlying claims principal.</param>
        public AnonymousClaimsFtpUser(ClaimsPrincipal principal)
            : base(principal)
        {
            Email = principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <inheritdoc />
        public string? Email { get; }
    }
}
