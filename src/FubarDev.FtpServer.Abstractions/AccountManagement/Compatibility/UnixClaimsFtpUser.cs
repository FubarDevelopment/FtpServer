// <copyright file="UnixClaimsFtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

namespace FubarDev.FtpServer.AccountManagement.Compatibility
{
    /// <summary>
    /// A <see cref="IUnixUser"/> implementation that uses the data from a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    [Obsolete("Will only be used for compatibility purposes.")]
    public class UnixClaimsFtpUser : ClaimsFtpUser, IUnixUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnixClaimsFtpUser"/> class.
        /// </summary>
        /// <param name="principal">The principal to initialize this object with.</param>
        public UnixClaimsFtpUser(ClaimsPrincipal principal)
            : base(principal)
        {
            HomePath = principal.FindFirst(FtpClaimTypes.HomePath)?.Value;
            UserId = ConvertToLong(principal.FindFirst(FtpClaimTypes.UserId)?.Value) ?? -1;
            GroupId = ConvertToLong(principal.FindFirst(FtpClaimTypes.GroupId)?.Value) ?? -1;
        }

        /// <inheritdoc />
        public string? HomePath { get; }

        /// <inheritdoc />
        public long UserId { get; }

        /// <inheritdoc />
        public long GroupId { get; }

        private static long? ConvertToLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Convert.ToInt64(value);
        }
    }
}
