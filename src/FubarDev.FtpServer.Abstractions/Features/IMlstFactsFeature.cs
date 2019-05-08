// <copyright file="IMlstFactsFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.FtpServer.ListFormatters.Facts;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features
{
    /// <summary>
    /// Active facts sent by <c>MLST</c> and <c>MLSD</c>.
    /// </summary>
    public interface IMlstFactsFeature
    {
        /// <summary>
        /// Gets the active <see cref="IFact"/> sent by <c>MLST</c> and <c>MLSD</c>.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        ISet<string> ActiveMlstFacts { get; }
    }
}
