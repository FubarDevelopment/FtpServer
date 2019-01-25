// <copyright file="IAuthenticationMechanism.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authentication
{
    /// <summary>
    /// The interface for an authentication mechanism.
    /// </summary>
    public interface IAuthenticationMechanism
    {
        /// <summary>
        /// Resets the authentication mechanism.
        /// </summary>
        [NotNull]
        void Reset();

        /// <summary>
        /// Returns a value indicating whether a mechanism with the given <paramref name="methodIdentifier"/> is supported by this implementor.
        /// </summary>
        /// <param name="methodIdentifier">The method identifier.</param>
        /// <returns><see langword="true"/> when the given <paramref name="methodIdentifier"/> is supported by this implementor.</returns>
        bool CanHandle([NotNull] string methodIdentifier);

        /// <summary>
        /// Processes the <c>AUTH</c> command.
        /// </summary>
        /// <remarks>
        /// If this handler doesn't support the passed <paramref name="methodIdentifier"/>, then it should
        /// respond with either 502 or 504.
        /// </remarks>
        /// <param name="methodIdentifier">The method identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response to return.</returns>
        [NotNull]
        [ItemNotNull]
        Task<FtpResponse> HandleAuthAsync([NotNull] string methodIdentifier, CancellationToken cancellationToken);

        /// <summary>
        /// Processes the <c>ADAT</c> command.
        /// </summary>
        /// <param name="data">The data passed to the ADAT command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response to return.</returns>
        [NotNull]
        [ItemNotNull]
        Task<FtpResponse> HandleAdatAsync([NotNull] byte[] data, CancellationToken cancellationToken);
    }
}
