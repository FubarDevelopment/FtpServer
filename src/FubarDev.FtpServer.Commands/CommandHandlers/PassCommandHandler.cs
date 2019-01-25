//-----------------------------------------------------------------------
// <copyright file="PassCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>PASS</c> command.
    /// </summary>
    public class PassCommandHandler : FtpCommandHandler
    {
        [NotNull]
        private readonly IEnumerable<IMembershipProvider> _membershipProviders;

        [NotNull]
        private readonly IFileSystemClassFactory _fileSystemClassFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="membershipProviders">The membership providers.</param>
        /// <param name="fileSystemClassFactory">The file system access factory.</param>
        public PassCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull] IEnumerable<IMembershipProvider> membershipProviders,
            [NotNull] IFileSystemClassFactory fileSystemClassFactory)
            : base(connectionAccessor, "PASS")
        {
            _membershipProviders = membershipProviders;
            _fileSystemClassFactory = fileSystemClassFactory;
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Connection.Data.User == null)
            {
                return new FtpResponse(530, T("No user name given"));
            }

            var password = command.Argument;
            foreach (var membershipProvider in _membershipProviders)
            {
                var validationResult = await membershipProvider.ValidateUserAsync(Connection.Data.User.Name, password)
                    .ConfigureAwait(false);
                if (validationResult.IsSuccess)
                {
                    Connection.Data.User = validationResult.User;

#pragma warning disable 618
                    Connection.Data.IsAnonymous = validationResult.User is IAnonymousFtpUser;
#pragma warning restore 618

                    Connection.Data.IsLoggedIn = true;
                    Connection.Data.AuthenticatedBy = membershipProvider;
                    Connection.Data.FileSystem = await _fileSystemClassFactory
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
    }
}
