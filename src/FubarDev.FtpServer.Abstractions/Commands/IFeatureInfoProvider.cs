// <copyright file="IFeatureInfoProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Provides feature information.
    /// </summary>
    public interface IFeatureInfoProvider
    {
        /// <summary>
        /// Get all feature information items that can be found in the system.
        /// </summary>
        /// <returns>The feature information items.</returns>
        IEnumerable<FoundFeatureInfo> GetFeatureInfoItems();
    }
}
