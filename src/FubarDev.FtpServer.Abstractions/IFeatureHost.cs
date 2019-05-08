// <copyright file="IFeatureHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for something that may host FTP features.
    /// </summary>
    public interface IFeatureHost
    {
        /// <summary>
        /// Gets a collection of features supported by this command handler.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <returns>A list of features supported by this command handler.</returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IFeatureInfo> GetSupportedFeatures([NotNull] IFtpConnection connection);
    }
}
