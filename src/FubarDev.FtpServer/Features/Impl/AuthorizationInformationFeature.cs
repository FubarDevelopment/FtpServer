// <copyright file="AuthorizationInformationFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Connections.Features;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IConnectionUserFeature"/>.
    /// </summary>
    internal class AuthorizationInformationFeature
        : IConnectionUserFeature,
            IResettableFeature,
#pragma warning disable 618
            IAuthorizationInformationFeature
#pragma warning restore 618
    {
        private readonly IFtpStatisticsCollectorFeature _statisticsCollectorFeature;
        private ClaimsPrincipal? _user;

        public AuthorizationInformationFeature(IFtpStatisticsCollectorFeature statisticsCollectorFeature)
        {
            _statisticsCollectorFeature = statisticsCollectorFeature;
        }

        /// <inheritdoc />
        ClaimsPrincipal? IAuthorizationInformationFeature.FtpUser
        {
            get => _user;
            set => SetUser(value);
        }

        /// <inheritdoc />
#nullable disable
        public ClaimsPrincipal User
        {
            get => _user;
            set => SetUser(value);
        }
#nullable restore

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken)
        {
            SetUser(null);
            return Task.CompletedTask;
        }

        private void SetUser(ClaimsPrincipal? user)
        {
            _user = user;
            _statisticsCollectorFeature.ForEach(collector => collector.UserChanged(user));
        }
    }
}
