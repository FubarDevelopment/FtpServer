//-----------------------------------------------------------------------
// <copyright file="PassCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class PassCommandHandler : FtpCommandHandler
    {
        public PassCommandHandler(FtpConnection connection)
            : base(connection, "PASS")
        {
        }

        public override bool IsLoginRequired => false;

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var password = command.Argument;
            if (Server.MembershipProvider.ValidateUser(Connection.Data.UserName, password))
            {
                Connection.Data.IsLoggedIn = true;
                Connection.Data.FileSystem = await Server.FileSystemClassFactory.Create(Connection.Data.UserName);
                Connection.Data.Path = new Stack<IUnixDirectoryEntry>();
                return new FtpResponse(220, "Password ok, FTP server ready");
            }
            return new FtpResponse(530, "Username or password incorrect");
        }
    }
}
