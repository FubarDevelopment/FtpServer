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
    /// Implements the <code>PASS</code> command.
    /// </summary>
    public class PassCommandHandler : FtpCommandHandler
    {
        [NotNull]
        private readonly IEnumerable<IBaseMembershipProvider> _membershipProviders;

        [NotNull]
        private readonly IFileSystemClassFactory _fileSystemClassFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        /// <param name="membershipProviders">The membership providers.</param>
        /// <param name="fileSystemClassFactory">The file system access factory.</param>
        public PassCommandHandler(
            [NotNull] IFtpConnection connection,
            [NotNull] IEnumerable<IBaseMembershipProvider> membershipProviders,
            [NotNull] IFileSystemClassFactory fileSystemClassFactory)
            : base(connection, "PASS")
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
                return new FtpResponse(530, "No user name given");
            }

            var password = command.Argument;
            foreach (var membershipProvider in _membershipProviders)
            {
                var validationResult = await ValidateUser(membershipProvider, Connection.Data.User.Name, password);

                if (validationResult.IsSuccess)
                {
                    var isAnonymous = validationResult.Status == MemberValidationStatus.Anonymous;
                    var userId = isAnonymous ? password : Connection.Data.User.Name;
                    Connection.Data.User = validationResult.User;
                    Connection.Data.IsLoggedIn = true;
                    Connection.Data.AuthenticatedBy = membershipProvider;
                    Connection.Data.IsAnonymous = isAnonymous;
                    Connection.Data.FileSystem = await _fileSystemClassFactory.Create(userId, isAnonymous).ConfigureAwait(false);
                    Connection.Data.Path = new Stack<IUnixDirectoryEntry>();
                    return new FtpResponse(230, "Password ok, FTP server ready");
                }
            }

            return new FtpResponse(530, "Username or password incorrect");
        }


        /// <summary>
        /// Validates the given combination of <paramref name="username"/> and <paramref name="password"/> against the selected <paramref name="membershipProvider"/>
        /// </summary>
        /// <param name="membershipProvider">The membership provider that is used for authenticating the given user. The membership provider must implement either <see cref="IMembershipProvider"/> or <see cref="IAsyncMembershipProvider"/>.</param>
        /// <param name="username">The name of the user that should be authenticated against the <paramref name="membershipProvider"/>.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>Returns the <see cref="MemberValidationResult"/> with the result of the validation.</returns>
        /// <exception cref="NotSupportedException">The given membership provider type is not supported.</exception>
        private async Task<MemberValidationResult> ValidateUser(IBaseMembershipProvider membershipProvider, string username, string password)
        {
            MemberValidationResult validationResult;
            switch (membershipProvider)
            {
                case IMembershipProvider syncMembershipProvider:
                    validationResult = syncMembershipProvider.ValidateUser(username, password);
                    break;
                case IAsyncMembershipProvider asyncMembershipProvider:
                    validationResult = await asyncMembershipProvider.ValidateUserAsync(username, password);
                    break;
                default:
                    throw new NotSupportedException("The given membership provider type is not supported.");
            }

            return validationResult;
        }
    }
}
