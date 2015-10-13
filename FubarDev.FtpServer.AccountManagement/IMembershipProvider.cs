//-----------------------------------------------------------------------
// <copyright file="IMembershipProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Membership provider interface
    /// </summary>
    /// <remarks>
    /// This interface must be implemented to allow the username/password authentication.
    /// </remarks>
    public interface IMembershipProvider
    {
        /// <summary>
        /// Validates if the combination of <paramref name="username"/> and <paramref name="password"/> is valid.
        /// </summary>
        /// <param name="username">The user name</param>
        /// <param name="password">The password</param>
        /// <returns><code>true</code> if the combination of <paramref name="username"/> and <paramref name="password"/> is valid</returns>
        bool ValidateUser(string username, string password);
    }
}
