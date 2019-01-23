//-----------------------------------------------------------------------
// <copyright file="SimpleMailAddressValidation.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    /// <summary>
    /// The password must have the form of a valid email address.
    /// </summary>
    /// <remarks>
    /// A <c>@</c> is required and the host name must contain a dot.
    /// </remarks>
    public class SimpleMailAddressValidation : IAnonymousPasswordValidator
    {
        /// <inheritdoc/>
        public bool IsValid(string password)
        {
            if (password.IndexOfAny(new[] { '/', '\\' }) != -1)
            {
                return false;
            }

            var atIndex = password.IndexOf('@');
            if (atIndex == -1)
            {
                return false;
            }

            if (atIndex < 3)
            {
                return false;
            }

            var domain = password.Substring(atIndex + 1);
            var dotIndex = domain.LastIndexOf('.');
            if (dotIndex == -1)
            {
                return false;
            }

            if (dotIndex < 3)
            {
                return false;
            }

            var topLevelDomain = domain.Substring(dotIndex + 1);
            return topLevelDomain.Length >= 2;
        }
    }
}
