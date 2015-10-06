//-----------------------------------------------------------------------
// <copyright file="AborCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class AborCommandHandler : FtpCommandHandler
    {
        public AborCommandHandler(FtpConnection connection)
            : base(connection, "ABOR")
        {
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (Data.BackgroundCommandHandler.Cancel())
            {
                return Task.FromResult(new FtpResponse(226, "File transfer aborting."));
            }

            return Task.FromResult(new FtpResponse(226, "Cannot abort - no active transfer."));
        }
    }
}