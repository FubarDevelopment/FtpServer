// <copyright file="SiteCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandExtensions;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <code>SITE</code> command handler
    /// </summary>
    public class SiteCommandHandler : FtpCommandHandler, IFtpCommandHandlerExtensionHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public SiteCommandHandler(FtpConnection connection)
            : base(connection, "SITE")
        {
            Extensions = new Dictionary<string, FtpCommandHandlerExtension>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public IDictionary<string, FtpCommandHandlerExtension> Extensions { get; }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
                return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));

            var argument = FtpCommand.Parse(command.Argument);
            FtpCommandHandlerExtension extension;
            if (!Extensions.TryGetValue(argument.Name, out extension))
                return Task.FromResult(new FtpResponse(500, "Syntax error, command unrecognized."));

            return extension.Process(argument, cancellationToken);
        }
    }
}
