//-----------------------------------------------------------------------
// <copyright file="NoTopLevelDomainValidation.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    /// <summary>
    /// Allows a server address for anonymous authentication without top level domain.
    /// </summary>
    /// <remarks>
    /// In other words: No dot required after <c>@</c>.
    /// </remarks>
    public class NoTopLevelDomainValidation : IAnonymousPasswordValidator
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

            return true;
        }
    }
}
