// <copyright file="SiteCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>SITE</c> command handler.
    /// </summary>
    [FtpCommandHandler("SITE")]
    public class SiteCommandHandler : FtpCommandHandler, IFtpCommandHandlerExtensionHost
    {
        /// <inheritdoc/>
        public IDictionary<string, IFtpCommandHandlerExtension> Extensions { get; set; }
            = new Dictionary<string, IFtpCommandHandlerExtension>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return Task.FromResult<IFtpResponse?>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }

            var argument = FtpCommand.Parse(command.Argument);
            if (!Extensions.TryGetValue(argument.Name, out var extension))
            {
                return Task.FromResult<IFtpResponse?>(new FtpResponse(500, T("Syntax error, command unrecognized.")));
            }

            return extension.Process(argument, cancellationToken);
        }
    }
}
