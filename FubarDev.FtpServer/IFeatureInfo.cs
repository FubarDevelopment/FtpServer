// <copyright file="IFeatureInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Defines a feature and its handler.
    /// </summary>
    public interface IFeatureInfo
    {
        /// <summary>
        /// The name of the feature
        /// </summary>
        /// <remarks>
        /// Used by the <code>OPTS</code> command to find the handler of the feature to modify.
        /// </remarks>
        [NotNull, ItemNotNull]
        ISet<string> Names { get; }

        /// <summary>
        /// Build an informational string to be sent by the <code>FEAT</code> command
        /// </summary>
        /// <param name="connection">The configured connection</param>
        /// <returns>the informational string to be sent by the <code>FEAT</code> command</returns>
        [NotNull]
        string BuildInfo([NotNull] FtpConnection connection);
    }
}
