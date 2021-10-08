//-----------------------------------------------------------------------
// <copyright file="AnonymousMembershipProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement.Anonymous;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Allow any anonymous login.
    /// </summary>
    public class AnonymousMembershipProvider : IMembershipProviderAsync
    {
        private readonly IAnonymousPasswordValidator _anonymousPasswordValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMembershipProvider"/> class.
        /// </summary>
        /// <remarks>
        /// Anonymous logins must provide an email address that at least seems to
        /// be valid (<see cref="SimpleMailAddressValidation"/>).
        /// </remarks>
        public AnonymousMembershipProvider()
            : this(new SimpleMailAddressValidation())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMembershipProvider"/> class.
        /// </summary>
        /// <param name="anonymousPasswordValidator">Anonymous login validation.</param>
        public AnonymousMembershipProvider(IAnonymousPasswordValidator anonymousPasswordValidator)
        {
            _anonymousPasswordValidator = anonymousPasswordValidator;
        }

        /// <summary>
        /// Create a claims principal for an anonymous (authenticated!) user.
        /// </summary>
        /// <param name="email">The anonymous users e-mail address.</param>
        /// <returns>The anonymous claims principal.</returns>
        public static ClaimsPrincipal CreateAnonymousPrincipal(string? email)
        {
            var anonymousClaims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, "anonymous"),
                new Claim(ClaimTypes.Anonymous, email ?? string.Empty),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "anonymous"),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "guest"),
                new Claim(ClaimTypes.AuthenticationMethod, "anonymous"),
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                anonymousClaims.Add(new Claim(ClaimTypes.Email, email, ClaimValueTypes.Email));
            }

            var identity = new ClaimsIdentity(anonymousClaims, "anonymous");
            var principal = new ClaimsPrincipal(identity);
            return principal;
        }

        /// <inheritdoc/>
        public Task<MemberValidationResult> ValidateUserAsync(
            string username,
            string password,
            CancellationToken cancellationToken)
        {
            if (string.Equals(username, "anonymous"))
            {
                if (_anonymousPasswordValidator.IsValid(password))
                {
                    return Task.FromResult(
                        new MemberValidationResult(MemberValidationStatus.Anonymous, CreateAnonymousPrincipal(password)));
                }

                return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidAnonymousEmail));
            }

            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
        }

        /// <inheritdoc />
        public Task LogOutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
        {
            return ValidateUserAsync(username, password, CancellationToken.None);
        }
    }
}
