// <copyright file="ClaimsFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

namespace FubarDev.FtpServer.AccountManagement.Compatibility
{
    /// <summary>
    /// Wrapper for <see cref="ClaimsPrincipal"/> to make it accessible via <see cref="IFtpUser"/>.
    /// </summary>
    [Obsolete("Will only be used for compatibility purposes.")]
    public class ClaimsFtpUser : IFtpUser
    {
        private readonly ClaimsPrincipal _principal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsFtpUser"/> class.
        /// </summary>
        /// <param name="principal">The underlying claims principal.</param>
        public ClaimsFtpUser(ClaimsPrincipal principal)
        {
            _principal = principal;
            Name = principal.Identity.Name;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool IsInGroup(string groupName)
        {
            return _principal.IsInRole(groupName);
        }
    }
}
