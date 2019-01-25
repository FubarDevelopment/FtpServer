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

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authorization
{
    public class PasswordAuthorization : IAuthorizationMechanism
    {
        [NotNull]
        [ItemNotNull]
        private readonly IReadOnlyCollection<IMembershipProvider> _membershipProviders;

        private string _userName;
        private bool _needsPassword = false;

        public PasswordAuthorization([NotNull, ItemNotNull] IEnumerable<IMembershipProvider> membershipProviders)
        {
            _membershipProviders = membershipProviders.ToList();
        }

        public Task<FtpResponse> HandleAcctAsync([NotNull] string account, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task<FtpResponse> HandlePassAsync([NotNull] string password, CancellationToken cancellationToken)
        {
            if (!_needsPassword)
            {
                return new FtpResponse(530, "No user name given");
            }

            throw new NotImplementedException();
        }

        public Task<FtpResponse> HandleUserAsync([NotNull] string userIdentifier, CancellationToken cancellationToken)
        {
            _userName = userIdentifier;
            _needsPassword = true;
            return Task.FromResult(new FtpResponse(331, $"User {userIdentifier} logged in, needs password"));
        }

        public void Reset([CanBeNull] IAuthenticationMechanism authenticationMechanism)
        {
            _needsPassword = false;
            _userName = null;
        }
    }
}
