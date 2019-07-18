//-----------------------------------------------------------------------
// <copyright file="OptsCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>OPTS</c> command.
    /// </summary>
    [FtpCommandHandler("OPTS")]
    public class OptsCommandHandler : FtpCommandHandler, IFtpCommandHandlerExtensionHost
    {
        /// <inheritdoc/>
        public IDictionary<string, IFtpCommandHandlerExtension> Extensions { get; set; }
            = new Dictionary<string, IFtpCommandHandlerExtension>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = FtpCommand.Parse(command.Argument);
            if (!Extensions.TryGetValue(argument.Name, out var extension))
            {
                return new FtpResponse(500, T("Syntax error, command unrecognized."));
            }

            return await extension.Process(argument, cancellationToken).ConfigureAwait(false);
        }
    }
}
