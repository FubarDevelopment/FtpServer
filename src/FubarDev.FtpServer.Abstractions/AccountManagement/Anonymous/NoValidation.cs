//-----------------------------------------------------------------------
// <copyright file="NoValidation.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    /// <summary>
    /// Performs no validation. Every kind of password is allowed.
    /// </summary>
    public class NoValidation : IAnonymousPasswordValidator
    {
        /// <inheritdoc/>
        public bool IsValid(string password)
        {
            if (password.IndexOfAny(new[] { '/', '\\' }) != -1)
            {
                return false;
            }

            return true;
        }
    }
}
