//-----------------------------------------------------------------------
// <copyright file="RestCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class RestCommandHandler : FtpCommandHandler
    {
        public RestCommandHandler(FtpConnection connection)
            : base(connection, "REST")
        {
            SupportedExtensions = new List<string>
            {
                "REST STREAM",
            };
        }

        public override IReadOnlyCollection<string> SupportedExtensions { get; }

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            Data.RestartPosition = Convert.ToInt64(command.Argument, 10);
            return Task.FromResult(new FtpResponse(350, $"Restarting next transfer from position {Data.RestartPosition}"));
        }
    }
}
