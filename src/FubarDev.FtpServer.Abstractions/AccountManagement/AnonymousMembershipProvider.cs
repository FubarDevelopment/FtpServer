//-----------------------------------------------------------------------
// <copyright file="AnonymousMembershipProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement.Anonymous;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Allow any anonymous login.
    /// </summary>
    public class AnonymousMembershipProvider : IMembershipProvider
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
        public AnonymousMembershipProvider([NotNull] IAnonymousPasswordValidator anonymousPasswordValidator)
        {
            _anonymousPasswordValidator = anonymousPasswordValidator;
        }

        /// <inheritdoc/>
        public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
        {
            if (string.Equals(username, "anonymous"))
            {
                if (_anonymousPasswordValidator.IsValid(password))
                {
                    return Task.FromResult(
                        new MemberValidationResult(MemberValidationStatus.Anonymous, new AnonymousFtpUser(password)));
                }

                return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidAnonymousEmail));
            }

            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
        }
    }
}
