// <copyright file="OptsMlstCommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;

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
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public OptsMlstCommandExtension([NotNull] IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "OPTS", "MLST")
        {
        }

        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
            Connection.Data.ActiveMlstFacts.Clear();
            foreach (var knownFact in MlstCommandHandler.KnownFacts)
            {
                Connection.Data.ActiveMlstFacts.Add(knownFact);
            }
        }

        /// <inheritdoc />
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var facts = command.Argument.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            Connection.Data.ActiveMlstFacts.Clear();
            foreach (var fact in facts)
            {
                if (!MlstCommandHandler.KnownFacts.Contains(fact))
                {
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
                }

                Connection.Data.ActiveMlstFacts.Add(fact);
            }
            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
