//-----------------------------------------------------------------------
// <copyright file="BlockAnonymousValidation.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    /// <summary>
    /// Disallow anonymous logins.
    /// </summary>
    public class BlockAnonymousValidation : IAnonymousPasswordValidator
    {
        /// <inheritdoc/>
        public bool IsValid(string password)
        {
            return false;
        }
    }
}
