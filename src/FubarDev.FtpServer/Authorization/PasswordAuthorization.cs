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
using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Authorization
{
    /// <summary>
    /// The default password authentication mechanism.
    /// </summary>
    public class PasswordAuthorization : AuthorizationMechanism
    {
        [NotNull]
        private readonly IFileSystemClassFactory _fileSystemFactory;

        [NotNull]
        [ItemNotNull]
        private readonly IReadOnlyCollection<IMembershipProvider> _membershipProviders;

        private string _userName;
        private bool _needsPassword;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordAuthorization"/> class.
        /// </summary>
        /// <param name="connection">The required FTP connection.</param>
        /// <param name="fileSystemFactory">The file system factory.</param>
        /// <param name="membershipProviders">The membership providers for password authorization.</param>
        public PasswordAuthorization(
            [NotNull] IFtpConnection connection,
            [NotNull] IFileSystemClassFactory fileSystemFactory,
            [NotNull, ItemNotNull] IEnumerable<IMembershipProvider> membershipProviders)
            : base(connection)
        {
            _fileSystemFactory = fileSystemFactory;
            _membershipProviders = membershipProviders.ToList();
        }

        /// <inheritdoc />
        public override Task<FtpResponse> HandleAcctAsync(string account, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(421, T("Service not available")));
        }

        /// <inheritdoc />
        public override async Task<FtpResponse> HandlePassAsync(string password, CancellationToken cancellationToken)
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
                    Connection.Data.User = validationResult.User;

#pragma warning disable 618
                    Connection.Data.IsAnonymous = validationResult.User is IAnonymousFtpUser;
                    Connection.Data.IsLoggedIn = true;
#pragma warning restore 618

                    Connection.Data.AuthenticatedBy = membershipProvider;
                    Connection.Data.FileSystem = await _fileSystemFactory
                       .Create(
                            new DefaultAccountInformation(
                                validationResult.User ?? throw new InvalidOperationException(T("The user property must be set if validation succeeded.")),
                                membershipProvider))
                       .ConfigureAwait(false);
                    Connection.Data.Path = new Stack<IUnixDirectoryEntry>();
                    return new FtpResponse(230, T("Password ok, FTP server ready"));
                }
            }

            return new FtpResponse(530, T("Username or password incorrect"));
        }

        /// <inheritdoc />
        public override Task<FtpResponse> HandleUserAsync(string userIdentifier, CancellationToken cancellationToken)
        {
            _userName = userIdentifier;
            _needsPassword = true;

            Connection.Data.User = new UnauthenticatedUser(userIdentifier);

            return Task.FromResult(new FtpResponse(331, T("User {0} logged in, needs password", userIdentifier)));
        }

        /// <inheritdoc />
        public override void Reset(IAuthenticationMechanism authenticationMechanism)
        {
            _needsPassword = false;
            _userName = null;
        }

        private class DefaultAccountInformation : IAccountInformation
        {
            public DefaultAccountInformation(IFtpUser user, IMembershipProvider authenticatedBy)
            {
                User = user;
                AuthenticatedBy = authenticatedBy;
            }

            /// <inheritdoc />
            public IFtpUser User { get; }

            /// <inheritdoc />
            public IMembershipProvider AuthenticatedBy { get; }
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
