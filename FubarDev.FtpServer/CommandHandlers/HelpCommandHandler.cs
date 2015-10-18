// <copyright file="HelpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <code>HELP</code> command handler
    /// </summary>
    public class HelpCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public HelpCommandHandler(FtpConnection connection)
            : base(connection, "HELP")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
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
            await Connection.WriteAsync("211-HELP", cancellationToken);

            await Connection.WriteAsync(" SITE BLST [DIRECT]", cancellationToken);

            return new FtpResponse(211, "HELP");
        }
    }
}
