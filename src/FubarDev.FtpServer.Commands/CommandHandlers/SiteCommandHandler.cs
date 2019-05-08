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
        [NotNull]
        private readonly IReadOnlyCollection<IFtpCommandHandlerExtension> _extensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteCommandHandler"/> class.
        /// </summary>
        /// <param name="extensions">All registered extensions.</param>
        public SiteCommandHandler(
            [NotNull, ItemNotNull] IEnumerable<IFtpCommandHandlerExtension> extensions)
            : base("SITE")
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
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures(IFtpConnection connection)
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
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
            }

            var argument = FtpCommand.Parse(command.Argument);
            if (!Extensions.TryGetValue(argument.Name, out var extension))
            {
                return Task.FromResult<IFtpResponse>(new FtpResponse(500, T("Syntax error, command unrecognized.")));
            }

            return extension.Process(argument, cancellationToken);
        }
    }
}
