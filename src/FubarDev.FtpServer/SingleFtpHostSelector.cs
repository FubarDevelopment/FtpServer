// <copyright file="SingleFtpHostSelector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;
using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// A simple implementation of the <see cref="IFtpHostSelector"/> interface.
    /// </summary>
    public class SingleFtpHostSelector : IFtpHostSelector
    {
        private readonly IFtpConnection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleFtpHostSelector"/> class.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="authenticationMechanisms">The registered authentication mechanisms.</param>
        /// <param name="authorizationMechanisms">The registered authorization mechanisms.</param>
        public SingleFtpHostSelector(
            IFtpConnection connection,
            IEnumerable<IAuthenticationMechanism> authenticationMechanisms,
            IEnumerable<IAuthorizationMechanism> authorizationMechanisms)
        {
            SelectedHost = new DefaultFtpHost(authenticationMechanisms.ToList(), authorizationMechanisms.ToList());
            _connection = connection;
        }

        /// <inheritdoc />
        public IFtpHost SelectedHost { get; }

        /// <inheritdoc />
        public Task<IFtpResponse> SelectHostAsync(HostInfo hostInfo, CancellationToken cancellationToken)
        {
            var localizationFeature = _connection.Features.Get<ILocalizationFeature>();
            return Task.FromResult<IFtpResponse>(new FtpResponse(504, localizationFeature.Catalog.GetString("Unknown host \"{0}\"", hostInfo)));
        }

        private class DefaultFtpHost : IFtpHost
        {
            public DefaultFtpHost(
                IReadOnlyCollection<IAuthenticationMechanism> authenticationMechanisms,
                IReadOnlyCollection<IAuthorizationMechanism> authorizationMechanisms)
            {
                AuthenticationMechanisms = authenticationMechanisms;
                AuthorizationMechanisms = authorizationMechanisms;
            }

            /// <inheritdoc />
            public HostInfo Info { get; } = new HostInfo();

            /// <inheritdoc />
            public IEnumerable<IAuthenticationMechanism> AuthenticationMechanisms { get; }

            /// <inheritdoc />
            public IEnumerable<IAuthorizationMechanism> AuthorizationMechanisms { get; }
        }
    }
}
