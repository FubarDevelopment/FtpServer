// <copyright file="OptsMlstCommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// <c>MLST</c> extension for the <c>OPTS</c> command.
    /// </summary>
    public class OptsMlstCommandExtension : FtpCommandHandlerExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptsMlstCommandExtension"/> class.
        /// </summary>
        public OptsMlstCommandExtension()
            : base("OPTS", "MLST")
        {
            // Don't announce this extension, because it gets already announced
            // by the MLST command itself.
            AnnouncementMode = ExtensionAnnouncementMode.Hidden;
        }

        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
            var factsFeature = new MlstFactsFeature();
            Connection.Features.Set<IMlstFactsFeature>(factsFeature);
            foreach (var knownFact in MlstCommandHandler.KnownFacts)
            {
                factsFeature.ActiveMlstFacts.Add(knownFact);
            }
        }

        /// <inheritdoc />
        public override Task<IFtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var facts = command.Argument.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var factsFeature = Connection.Features.Get<IMlstFactsFeature>();
            factsFeature.ActiveMlstFacts.Clear();
            foreach (var fact in facts)
            {
                if (!MlstCommandHandler.KnownFacts.Contains(fact))
                {
                    return Task.FromResult<IFtpResponse>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
                }

                factsFeature.ActiveMlstFacts.Add(fact);
            }
            return Task.FromResult<IFtpResponse>(new FtpResponse(200, T("Command okay.")));
        }
    }
}
