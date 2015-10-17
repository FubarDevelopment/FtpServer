// <copyright file="HelpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class HelpCommandHandler : FtpCommandHandler
    {
        public HelpCommandHandler(FtpConnection connection)
            : base(connection, "HELP")
        {
        }

        public override bool IsLoginRequired => false;

        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var helpArg = command.Argument;
            if (string.IsNullOrEmpty(helpArg))
                helpArg = "SITE";

            switch (helpArg)
            {
                case "SITE":
                    return ShowHelpSite(cancellationToken);
                default:
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
            }
        }

        private async Task<FtpResponse> ShowHelpSite(CancellationToken cancellationToken)
        {
            await Connection.Write($"211-HELP", cancellationToken);

            await Connection.Write($" SITE BLST [DIRECT]", cancellationToken);

            return new FtpResponse(211, "HELP");
        }
    }
}
