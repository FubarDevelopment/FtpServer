// <copyright file="SingleFtpHostSelector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Options;

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
        /// <param name="authTlsOptions">The options for the AUTH TLS command.</param>
        public SingleFtpHostSelector(
            IFtpConnection connection,
            IEnumerable<IAuthenticationMechanism> authenticationMechanisms,
            IEnumerable<IAuthorizationMechanism> authorizationMechanisms,
            IOptions<AuthTlsOptions> authTlsOptions)
        {
            SelectedHost = new DefaultFtpHost(
                authenticationMechanisms.ToList(),
                authorizationMechanisms.ToList(),
                authTlsOptions);
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
                IReadOnlyCollection<IAuthorizationMechanism> authorizationMechanisms,
                IOptions<AuthTlsOptions> authTlsOptions)
            {
                AuthenticationMechanisms = authenticationMechanisms;
                AuthorizationMechanisms = authorizationMechanisms;
                Certificate = authTlsOptions.Value.ServerCertificate;
            }

            /// <inheritdoc />
            public HostInfo Info { get; } = new HostInfo();

            /// <inheritdoc />
            public X509Certificate? Certificate { get; }

            /// <inheritdoc />
            public IEnumerable<IAuthenticationMechanism> AuthenticationMechanisms { get; }

            /// <inheritdoc />
            public IEnumerable<IAuthorizationMechanism> AuthorizationMechanisms { get; }
        }
    }
}
