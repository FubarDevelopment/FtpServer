//-----------------------------------------------------------------------
// <copyright file="AnonymousMembershipProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using FubarDev.FtpServer.AccountManagement.Anonymous;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Allow any anonymous login
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
        /// <param name="anonymousPasswordValidator">Anonymous login validation</param>
        public AnonymousMembershipProvider(IAnonymousPasswordValidator anonymousPasswordValidator)
        {
            _anonymousPasswordValidator = anonymousPasswordValidator;
        }

        /// <inheritdoc/>
        public bool ValidateUser(string username, string password)
        {
            if (string.Equals(username, "anonymous"))
            {
                return _anonymousPasswordValidator.IsValid(password);
            }

            return false;
        }
    }
}
