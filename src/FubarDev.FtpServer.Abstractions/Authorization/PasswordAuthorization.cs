// <copyright file="PasswordAuthorization.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Compatibility;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Localization;

namespace FubarDev.FtpServer.Authorization
{
    /// <summary>
    /// The default password authentication mechanism.
    /// </summary>
    public class PasswordAuthorization : AuthorizationMechanism
    {
        private readonly IFtpServerMessages _serverMessages;

        private readonly IReadOnlyCollection<IAuthorizationAction> _authorizationActions;

        private readonly IReadOnlyCollection<IMembershipProvider> _membershipProviders;

        private string? _userName;
        private bool _needsPassword;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordAuthorization"/> class.
        /// </summary>
        /// <param name="connection">The required FTP connection.</param>
        /// <param name="membershipProviders">The membership providers for password authorization.</param>
        /// <param name="authorizationActions">Actions to be executed upon authorization.</param>
        /// <param name="serverMessages">The FTP server messages.</param>
        public PasswordAuthorization(
            IFtpConnection connection,
            IEnumerable<IMembershipProvider> membershipProviders,
            IEnumerable<IAuthorizationAction> authorizationActions,
            IFtpServerMessages serverMessages)
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
            if (!_needsPassword || _userName == null)
            {
                return new FtpResponse(530, T("No user name given"));
            }

            foreach (var membershipProvider in _membershipProviders)
            {
#pragma warning disable 618
                var validationResult = membershipProvider is IMembershipProviderAsync membershipProviderAsync
                    ? await membershipProviderAsync
                       .ValidateUserAsync(_userName, password, cancellationToken)
                       .ConfigureAwait(false)
                    : await membershipProvider
                       .ValidateUserAsync(_userName, password)
                       .ConfigureAwait(false);
#pragma warning restore 618
                if (validationResult.IsSuccess)
                {
                    var accountInformation = new DefaultAccountInformation(
                        validationResult.FtpUser,
                        membershipProvider);

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

            var authInfoFeature = Connection.Features.Get<IAuthorizationInformationFeature>();
#pragma warning disable 618
#pragma warning disable 612
            authInfoFeature.User = new UnauthenticatedUser(userIdentifier);
#pragma warning restore 612
#pragma warning restore 618
            authInfoFeature.FtpUser = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, userIdentifier),
                    }));

            return Task.FromResult<IFtpResponse>(new FtpResponse(331, T("User {0} logged in, needs password", userIdentifier)));
        }

        /// <inheritdoc />
        public override void Reset(IAuthenticationMechanism? authenticationMechanism)
        {
            _needsPassword = false;
            _userName = null;
        }

        [Obsolete]
        internal class UnauthenticatedUser : IFtpUser
        {
            public UnauthenticatedUser(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public bool IsInGroup(string groupName) => false;
        }

        private class DefaultAccountInformation : IAccountInformation
        {
            public DefaultAccountInformation(
                ClaimsPrincipal user,
                IMembershipProvider membershipProvider)
            {
                MembershipProvider = membershipProvider;
                FtpUser = user;
#pragma warning disable 618
#pragma warning disable 612
                User = user.CreateUser();
#pragma warning restore 612
#pragma warning restore 618
            }

            /// <inheritdoc />
            [Obsolete]
            public IFtpUser User { get; }

            /// <inheritdoc />
            public ClaimsPrincipal FtpUser { get; }

            /// <inheritdoc />
            public IMembershipProvider MembershipProvider { get; }
        }
    }
}
