//-----------------------------------------------------------------------
// <copyright file="UserCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class UserCommandHandler : FtpCommandHandler
    {
        public UserCommandHandler(FtpConnection connection)
            : base(connection, "USER")
        {
        }

        public override bool IsLoginRequired => false;

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var userName = command.Argument;
            Connection.Data.UserName = userName;
            return Task.FromResult(new FtpResponse(331, $"User {userName} logged in, needs password"));
        }
    }
}
