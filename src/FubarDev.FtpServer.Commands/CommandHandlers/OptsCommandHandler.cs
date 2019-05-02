//-----------------------------------------------------------------------
// <copyright file="OptsCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>OPTS</c> command.
    /// </summary>
    public class OptsCommandHandler : FtpCommandHandler, IFtpCommandHandlerExtensionHost
    {
        private readonly IReadOnlyCollection<IFtpCommandHandlerExtension> _extensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptsCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        /// <param name="extensions">All registered extensions.</param>
        public OptsCommandHandler(
            [NotNull] IFtpConnectionAccessor connectionAccessor,
            [NotNull, ItemNotNull] IEnumerable<IFtpCommandHandlerExtension> extensions)
            : base(connectionAccessor, "OPTS")
        {
            _extensions = extensions
               .Where(x => Names.Any(name => string.Equals(name, x.ExtensionFor, StringComparison.OrdinalIgnoreCase)))
               .ToList();
            Extensions = _extensions
                .SelectMany(x => x.Names.Select(n => new { Name = n, Extension = x }))
                .ToDictionary(x => x.Name, x => x.Extension, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public IDictionary<string, IFtpCommandHandlerExtension> Extensions { get; }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            foreach (var extension in _extensions)
            {
                var featureString = extension.ToFeatureString();
                if (string.IsNullOrEmpty(featureString))
                {
                    continue;
                }

                yield return new GenericFeatureInfo(
                    extension.Names.First(),
                    conn => featureString,
                    extension.IsLoginRequired ?? IsLoginRequired,
                    extension.Names.Skip(1).ToArray());
            }
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
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
