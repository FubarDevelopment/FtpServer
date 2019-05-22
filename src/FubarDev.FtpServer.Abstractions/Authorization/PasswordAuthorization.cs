// <copyright file="PasswordAuthorization.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Localization;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authorization
{
    /// <summary>
    /// The default password authentication mechanism.
    /// </summary>
    public class PasswordAuthorization : AuthorizationMechanism
    {
        [NotNull]
        private readonly IFtpServerMessages _serverMessages;

        [NotNull]
        [ItemNotNull]
        private readonly IReadOnlyCollection<IAuthorizationAction> _authorizationActions;

        [NotNull]
        [ItemNotNull]
        private readonly IReadOnlyCollection<IMembershipProvider> _membershipProviders;

        private string _userName;
        private bool _needsPassword;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordAuthorization"/> class.
        /// </summary>
        /// <param name="connection">The required FTP connection.</param>
        /// <param name="membershipProviders">The membership providers for password authorization.</param>
        /// <param name="authorizationActions">Actions to be executed upon authorization.</param>
        /// <param name="serverMessages">The FTP server messages.</param>
        public PasswordAuthorization(
            [NotNull] IFtpConnection connection,
            [NotNull, ItemNotNull] IEnumerable<IMembershipProvider> membershipProviders,
            [NotNull, ItemNotNull] IEnumerable<IAuthorizationAction> authorizationActions,
            [NotNull] IFtpServerMessages serverMessages)
            : base(connection)
        {
            _serverMessages = serverMessages;
            _authorizationActions = authorizationActions.OrderByDescending(x => x.Level).ToList();
            _membershipProviders = membershipProviders.ToList();
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> HandleAcctAsync(string account, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse>(new FtpResponse(421, T("Service not available")));
        }

        /// <inheritdoc />
        public override async Task<IFtpResponse> HandlePassAsync(string password, CancellationToken cancellationToken)
        {
            if (!_needsPassword)
            {
                return new FtpResponse(530, T("No user name given"));
            }

            foreach (var membershipProvider in _membershipProviders)
            {
                var validationResult = await membershipProvider
                   .ValidateUserAsync(_userName, password)
                   .ConfigureAwait(false);
                if (validationResult.IsSuccess)
                {
                    var accountInformation = new DefaultAccountInformation(
                        validationResult.User ?? throw new InvalidOperationException(
                            T("The user property must be set if validation succeeded.")));

                    foreach (var authorizationAction in _authorizationActions)
                    {
                        await authorizationAction.AuthorizedAsync(accountInformation, cancellationToken)
                           .ConfigureAwait(false);
                    }

                    return new FtpResponseTextBlock(
                        230,
                        _serverMessages.GetPasswordAuthorizationSuccessfulMessage(accountInformation));
                }
            }

            return new FtpResponse(530, T("Username or password incorrect"));
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> HandleUserAsync(string userIdentifier, CancellationToken cancellationToken)
        {
            _userName = userIdentifier;
            _needsPassword = true;

            Connection.Features.Get<IAuthorizationInformationFeature>().User = new UnauthenticatedUser(userIdentifier);

            return Task.FromResult<IFtpResponse>(new FtpResponse(331, T("User {0} logged in, needs password", userIdentifier)));
        }

        /// <inheritdoc />
        public override void Reset(IAuthenticationMechanism authenticationMechanism)
        {
            _needsPassword = false;
            _userName = null;
        }

        private class DefaultAccountInformation : IAccountInformation
        {
            public DefaultAccountInformation(IFtpUser user)
            {
                User = user;
            }

            /// <inheritdoc />
            public IFtpUser User { get; }
        }

        private class UnauthenticatedUser : IFtpUser
        {
            public UnauthenticatedUser(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public bool IsInGroup(string groupName) => false;
        }
    }
}
