// <copyright file="SiteCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// The <c>SITE</c> command handler.
    /// </summary>
    public class SiteCommandHandler : FtpCommandHandler, IFtpCommandHandlerExtensionHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="extensions">All registered extensions.</param>
        public SiteCommandHandler(
            IFtpConnectionAccessor connectionAccessor,
            [NotNull, ItemNotNull] IEnumerable<IFtpCommandHandlerExtension> extensions)
            : base(connectionAccessor, "SITE")
        {
            Extensions = extensions
                .Where(x => Names.Any(name => string.Equals(name, x.ExtensionFor, StringComparison.OrdinalIgnoreCase)))
                .SelectMany(x => x.Names.Select(n => new { Name = n, Extension = x }))
                .ToDictionary(x => x.Name, x => x.Extension, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public IDictionary<string, IFtpCommandHandlerExtension> Extensions { get; }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
            }

            var argument = FtpCommand.Parse(command.Argument);
            if (!Extensions.TryGetValue(argument.Name, out var extension))
            {
                return Task.FromResult(new FtpResponse(500, "Syntax error, command unrecognized."));
            }

            return extension.Process(argument, cancellationToken);
        }
    }
}
