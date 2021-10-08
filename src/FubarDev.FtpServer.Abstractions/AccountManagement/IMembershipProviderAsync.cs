// <copyright file="IMembershipProviderAsync.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Membership provider interface.
    /// </summary>
    /// <remarks>
    /// This interface must be implemented to allow the username/password authentication.
    /// </remarks>
    public interface IMembershipProviderAsync : IMembershipProvider
    {
        /// <summary>
        /// Validates if the combination of <paramref name="username"/> and <paramref name="password"/> is valid.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        Task<MemberValidationResult> ValidateUserAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Logout of the given <paramref name="principal"/>.
        /// </summary>
        /// <param name="principal">The principal to be logged out.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The task.</returns>
        Task LogOutAsync(
            ClaimsPrincipal principal,
            CancellationToken cancellationToken = default);
    }
}
