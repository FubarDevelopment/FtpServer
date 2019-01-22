//-----------------------------------------------------------------------
// <copyright file="PassCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

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
        private readonly IEnumerable<IMembershipProvider> _membershipProviders;

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
            [NotNull] IEnumerable<IMembershipProvider> membershipProviders,
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
                var validationResult = await membershipProvider.ValidateUserAsync(Connection.Data.User.Name, password)
                    .ConfigureAwait(false);
                if (validationResult.IsSuccess)
                {
                    var isAnonymous = validationResult.Status == MemberValidationStatus.Anonymous;
                    var userId = isAnonymous ? password : validationResult.User.Name;
                    Connection.Data.User = validationResult.User;
                    Connection.Data.IsLoggedIn = true;
                    Connection.Data.AuthenticatedBy = membershipProvider;
                    Connection.Data.IsAnonymous = isAnonymous;
                    Connection.Data.FileSystem = await _fileSystemClassFactory
                        .Create(validationResult.User, isAnonymous)
                        .ConfigureAwait(false);
                    Connection.Data.Path = new Stack<IUnixDirectoryEntry>();
                    return new FtpResponse(230, "Password ok, FTP server ready");
                }
            }

            return new FtpResponse(530, "Username or password incorrect");
        }
    }
}
