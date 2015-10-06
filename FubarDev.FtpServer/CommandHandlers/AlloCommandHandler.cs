//-----------------------------------------------------------------------
// <copyright file="AlloCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class AlloCommandHandler : FtpCommandHandler
    {
        public AlloCommandHandler(FtpConnection connection)
            : base(connection, "ALLO")
        {
        }

        public override bool IsLoginRequired => false;

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(202, "Allo processed successfully (deprecated)."));
        }
    }
}
