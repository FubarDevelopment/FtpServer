//-----------------------------------------------------------------------
// <copyright file="NoOpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class NoOpCommandHandler : FtpCommandHandler
    {
        public NoOpCommandHandler(FtpConnection connection)
            : base(connection, "NOOP")
        {
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(220, "NOOP command successful."));
        }
    }
}
