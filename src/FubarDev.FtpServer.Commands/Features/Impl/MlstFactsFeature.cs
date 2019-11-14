// <copyright file="MlstFactsFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandHandlers;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IMlstFactsFeature"/>.
    /// </summary>
    internal class MlstFactsFeature : IMlstFactsFeature, IResettableFeature
    {
        /// <inheritdoc />
        public ISet<string> ActiveMlstFacts { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken)
        {
            ActiveMlstFacts.Clear();
            foreach (var knownFact in MlstCommandHandler.KnownFacts)
            {
                ActiveMlstFacts.Add(knownFact);
            }

            return Task.CompletedTask;
        }
    }
}
