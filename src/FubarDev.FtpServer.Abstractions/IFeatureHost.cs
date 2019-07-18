// <copyright file="IFeatureHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for something that may host FTP features.
    /// </summary>
    [Obsolete("FTP command handlers (and other types) are now annotated with attributes implementing IFeatureInfo.")]
    public interface IFeatureHost
    {
        /// <summary>
        /// Gets a collection of features supported by this command handler.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <returns>A list of features supported by this command handler.</returns>
        [Obsolete("FTP command handlers (and other types) are now annotated with attributes implementing IFeatureInfo.")]
        IEnumerable<IFeatureInfo> GetSupportedFeatures(IFtpConnection connection);
    }
}
