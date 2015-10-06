//-----------------------------------------------------------------------
// <copyright file="StruCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class StruCommandHandler : FtpCommandHandler
    {
        public StruCommandHandler(FtpConnection connection)
            : base(connection, "STRU")
        {
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Argument, "F", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(new FtpResponse(220, "Structure set to File."));
            return Task.FromResult(new FtpResponse(504, $"File structure {command.Argument} not supported."));
        }
    }
}
