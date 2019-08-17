// <copyright file="AuthorizationMechanism.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Features;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authorization
{
    /// <summary>
    /// The base class for an authorization mechanism.
    /// </summary>
    public abstract class AuthorizationMechanism : IAuthorizationMechanism
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationMechanism"/> class.
        /// </summary>
        /// <param name="connection">The required FTP connection.</param>
        protected AuthorizationMechanism(IFtpConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Gets the FTP connection.
        /// </summary>
        public IFtpConnection Connection { get; }

        /// <inheritdoc />
        public abstract void Reset(IAuthenticationMechanism? authenticationMechanism);

        /// <inheritdoc />
        public abstract Task<IFtpResponse> HandleUserAsync(string userIdentifier, CancellationToken cancellationToken);

        /// <inheritdoc />
        public abstract Task<IFtpResponse> HandlePassAsync(string password, CancellationToken cancellationToken);

        /// <inheritdoc />
        public abstract Task<IFtpResponse> HandleAcctAsync(string account, CancellationToken cancellationToken);

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <returns>The translated message.</returns>
        protected string T(string message)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message);
        }

        /// <summary>
        /// Translates a message using the current catalog of the active connection.
        /// </summary>
        /// <param name="message">The message to translate.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The translated message.</returns>
        [StringFormatMethod("message")]
        protected string T(string message, params object[] args)
        {
            return Connection.Features.Get<ILocalizationFeature>().Catalog.GetString(message, args);
        }
    }
}
