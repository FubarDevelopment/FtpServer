//-----------------------------------------------------------------------
// <copyright file="ModeCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class ModeCommandHandler : FtpCommandHandler
    {
        public ModeCommandHandler(FtpConnection connection)
            : base(connection, "MODE")
        {
        }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Argument, "S", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(new FtpResponse(220, "Mode set to Stream."));
            return Task.FromResult(new FtpResponse(504, $"Transfer mode {command.Argument} not supported."));
        }
    }
}
