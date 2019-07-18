// <copyright file="HelpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>HELP</c> command handler.
    /// </summary>
    [FtpCommandHandler("HELP", isLoginRequired: false)]
    public class HelpCommandHandler : FtpCommandHandler
    {
        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var helpArg = command.Argument;
            if (string.IsNullOrEmpty(helpArg))
            {
                helpArg = "SITE";
            }

            switch (helpArg)
            {
                case "SITE":
                    return ShowHelpSiteAsync();
                default:
                    return Task.FromResult<IFtpResponse?>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }
        }

        private Task<IFtpResponse?> ShowHelpSiteAsync()
        {
            var helpText = new[]
            {
                "SITE BLST [DIRECT]",
            };

            return Task.FromResult<IFtpResponse?>(
                new FtpResponseList(
                    211,
                    "HELP",
                    "HELP",
                    helpText));
        }
    }
}
