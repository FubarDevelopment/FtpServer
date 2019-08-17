// <copyright file="OptsMlstCommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Features;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// <c>MLST</c> extension for the <c>OPTS</c> command.
    /// </summary>
    /// <remarks>
    /// Don't announce this extension, because it gets already announced
    /// by the MLST command itself.
    /// </remarks>
    [FtpCommandHandlerExtension("MLST", "OPTS")]
    public class OptsMlstCommandExtension : FtpCommandHandlerExtension
    {
        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
            Connection.Features.Set(MlstCommandHandler.CreateMlstFactsFeature());
        }

        /// <inheritdoc />
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var facts = command.Argument.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var factsFeature = Connection.Features.Get<IMlstFactsFeature>();
            factsFeature.ActiveMlstFacts.Clear();
            foreach (var fact in facts)
            {
                if (!MlstCommandHandler.KnownFacts.Contains(fact))
                {
                    return Task.FromResult<IFtpResponse?>(new FtpResponse(501, T("Syntax error in parameters or arguments.")));
                }

                factsFeature.ActiveMlstFacts.Add(fact);
            }
            return Task.FromResult<IFtpResponse?>(new FtpResponse(200, T("Command okay.")));
        }
    }
}
