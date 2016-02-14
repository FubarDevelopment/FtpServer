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

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>PASS</code> command.
    /// </summary>
    public class PassCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PassCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public PassCommandHandler(FtpConnection connection)
            : base(connection, "PASS")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Connection.Data.User == null)
                return new FtpResponse(530, "No user name given");
            var password = command.Argument;
            var validationResult = Server.MembershipProvider.ValidateUser(Connection.Data.User.Name, password);
            if (validationResult.IsSuccess)
            {
                var isAnonymous = validationResult.Status == MemberValidationStatus.Anonymous;
                var userId = isAnonymous ? password : Connection.Data.User.Name;
                Connection.Data.User = validationResult.User;
                Connection.Data.IsLoggedIn = true;
                Connection.Data.IsAnonymous = isAnonymous;
                Connection.Data.FileSystem = await Server.FileSystemClassFactory.Create(userId, isAnonymous);
                Connection.Data.Path = new Stack<IUnixDirectoryEntry>();
                return new FtpResponse(230, "Password ok, FTP server ready");
            }
            return new FtpResponse(530, "Username or password incorrect");
        }
    }
}
