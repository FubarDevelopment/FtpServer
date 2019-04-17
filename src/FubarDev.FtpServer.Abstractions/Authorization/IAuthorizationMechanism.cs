// <copyright file="IAuthorizationMechanism.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authorization
{
    /// <summary>
    /// Interface for all authorization mechanisms.
    /// </summary>
    public interface IAuthorizationMechanism
    {
        /// <summary>
        /// Resets the authorization mechanism.
        /// </summary>
        /// <param name="authenticationMechanism">The previously selected authentication mechanism.</param>
        void Reset([CanBeNull] IAuthenticationMechanism authenticationMechanism);

        /// <summary>
        /// Handles the <c>USER</c> command.
        /// </summary>
        /// <param name="userIdentifier">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the FTP response.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IFtpResponse> HandleUserAsync([NotNull] string userIdentifier, CancellationToken cancellationToken);

        /// <summary>
        /// Handles the <c>PASS</c> command.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the FTP response.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IFtpResponse> HandlePassAsync([NotNull] string password, CancellationToken cancellationToken);

        /// <summary>
        /// Handles the <c>ACCT</c> command.
        /// </summary>
        /// <param name="account">The account to select.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task returning the FTP response.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IFtpResponse> HandleAcctAsync([NotNull] string account, CancellationToken cancellationToken);
    }
}
