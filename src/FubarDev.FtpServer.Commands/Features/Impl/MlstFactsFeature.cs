// <copyright file="MlstFactsFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IMlstFactsFeature"/>.
    /// </summary>
    internal class MlstFactsFeature : IMlstFactsFeature
    {
        /// <inheritdoc />
        public ISet<string> ActiveMlstFacts { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
