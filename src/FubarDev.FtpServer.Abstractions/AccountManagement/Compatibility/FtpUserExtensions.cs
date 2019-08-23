// <copyright file="FtpUserExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

using FubarDev.FtpServer.Authorization;

namespace FubarDev.FtpServer.AccountManagement.Compatibility
{
    /// <summary>
    /// Extension methods for <see cref="IFtpUser"/>.
    /// </summary>
    [Obsolete("Will only be used for compatibility purposes.")]
    public static class FtpUserExtensions
    {
        /// <summary>
        /// Create a claims principal for an <see cref="IFtpUser"/>.
        /// </summary>
        /// <param name="user">The user to create the claims principal for.</param>
        /// <returns>The new claims principal.</returns>
        [Obsolete]
        public static ClaimsPrincipal CreateClaimsPrincipal(this IFtpUser user)
        {
            return new ClaimsPrincipalUser(user);
        }

        [Obsolete]
        private class ClaimsIdentityUser : ClaimsIdentity
        {
            public ClaimsIdentityUser(IFtpUser user)
                : base(CreateClaims(user), GetAuthenticationType(user))
            {
            }

            private static string? GetAuthenticationType(IFtpUser user)
            {
                return user switch
                {
                    IAnonymousFtpUser _ => "anonymous",
                    IUnixUser _ => "pam",
                    PasswordAuthorization.UnauthenticatedUser _ => null,
                    _ => "custom"
                };
            }

            private static IEnumerable<Claim> CreateClaims(IFtpUser user)
            {
                yield return new Claim(DefaultNameClaimType, "anonymous");

                switch (user)
                {
                    case IAnonymousFtpUser anonymousFtpUser:
                        yield return new Claim(DefaultRoleClaimType, "guest");
                        yield return new Claim(DefaultRoleClaimType, "anonymous");
                        yield return new Claim(ClaimTypes.Anonymous, anonymousFtpUser.Email ?? string.Empty);
                        yield return new Claim(ClaimTypes.AuthenticationMethod, "anonymous");
                        break;
                    case IUnixUser unixUser:
                        yield return new Claim(FtpClaimTypes.HomePath, $"{unixUser.HomePath}");
                        yield return new Claim(FtpClaimTypes.UserId, $"{unixUser.UserId}");
                        yield return new Claim(FtpClaimTypes.GroupId, $"{unixUser.GroupId}");
                        break;
                }
            }
        }

        [Obsolete]
        private class ClaimsPrincipalUser : ClaimsPrincipal
        {
            private readonly IFtpUser _user;
            private readonly ClaimsIdentityUser _identity;
            private readonly List<ClaimsIdentity> _identities = new List<ClaimsIdentity>();

            public ClaimsPrincipalUser(IFtpUser user)
            {
                _user = user;
                _identity = new ClaimsIdentityUser(user);
            }

            /// <inheritdoc />
            public override IIdentity Identity => _identity;

            /// <inheritdoc />
            public override IEnumerable<ClaimsIdentity> Identities => _identities;

            /// <inheritdoc />
            public override void AddIdentity(ClaimsIdentity identity)
            {
                _identities.Add(identity);
            }

            /// <inheritdoc />
            public override void AddIdentities(IEnumerable<ClaimsIdentity> identities)
            {
                _identities.AddRange(identities);
            }

            /// <inheritdoc />
            public override bool IsInRole(string role)
            {
                if (base.IsInRole(role))
                {
                    return true;
                }

                if (!_user.IsInGroup(role))
                {
                    return false;
                }

                _identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
                return true;
            }
        }
    }
}
