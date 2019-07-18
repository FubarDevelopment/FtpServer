// <copyright file="IFeatureInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Defines a feature and its handler.
    /// </summary>
    public interface IFeatureInfo
    {
        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        /// <remarks>
        /// Used by the <c>OPTS</c> command to find the handler of the feature to modify.
        /// </remarks>
        [Obsolete("Features don't have names. Use an attribute that implements IFeatureInfo, like - for example - FtpFeatureTextAttribute.")]
        ISet<string> Names { get; }

        /// <summary>
        /// Gets a value indicating whether this extension requires authentication.
        /// </summary>
        [Obsolete("This requirement is automatically determined through the FTP command handler.")]
        bool RequiresAuthentication { get; }

        /// <summary>
        /// Build an informational string to be sent by the <c>FEAT</c> command.
        /// </summary>
        /// <param name="connection">The configured connection.</param>
        /// <returns>the informational string to be sent by the <c>FEAT</c> command.</returns>
        [Obsolete("Use BuildInfo(object, IFtpConnection) instead.")]
        string BuildInfo(IFtpConnection connection);

        /// <summary>
        /// Build an informational string to be sent by the <c>FEAT</c> command.
        /// </summary>
        /// <param name="reference">The reference object type (e.g. an FTP command handler).</param>
        /// <param name="connection">The configured connection.</param>
        /// <returns>the informational strings to be sent by the <c>FEAT</c> command.</returns>
        IEnumerable<string> BuildInfo(Type reference, IFtpConnection connection);
    }
}
