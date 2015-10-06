//-----------------------------------------------------------------------
// <copyright file="SystCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class SystCommandHandler : FtpCommandHandler
    {
        public SystCommandHandler(FtpConnection connection)
            : base(connection, "SYST")
        {
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(200, $"{Server.OperatingSystem} Type: {Connection.Data.TransferMode}"));
        }
    }
}
