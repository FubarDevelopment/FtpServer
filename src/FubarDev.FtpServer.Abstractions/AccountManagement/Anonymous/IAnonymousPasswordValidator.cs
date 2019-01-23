//-----------------------------------------------------------------------
// <copyright file="IAnonymousPasswordValidator.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement.Anonymous
{
    /// <summary>
    /// This interface validates the password for an anonymous login.
    /// </summary>
    public interface IAnonymousPasswordValidator
    {
        /// <summary>
        /// Determines whether this password is valid for an anonymous login.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns><code>true</code> when the password is valid for this kind of anonymous authentication.</returns>
        bool IsValid(string password);
    }
}
