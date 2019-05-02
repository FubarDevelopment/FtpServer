// <copyright file="IAuthorizationAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.Authorization
{
    /// <summary>
    /// Interface for actions that need to be executed upon successful authorization.
    /// </summary>
    public interface IAuthorizationAction
    {
        /// <summary>
        /// Gets the level of importance.
        /// </summary>
        /// <remarks>
        /// Authorization actions with a higher level are executed first.
        /// The levels 1000 (inclusive) to 2000 (inclusive) are used internally
        /// to fill the connection information.
        /// </remarks>
        int Level { get; }

        /// <summary>
        /// Notification of successful authorization.
        /// </summary>
        /// <param name="accountInformation">The account information that led to successful authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        Task AuthorizedAsync(IAccountInformation accountInformation, CancellationToken cancellationToken);
    }
}
