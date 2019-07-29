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
        /// Build an informational string to be sent by the <c>FEAT</c> command.
        /// </summary>
        /// <param name="reference">The reference object type (e.g. an FTP command handler).</param>
        /// <param name="connectionContext">The configured connection.</param>
        /// <returns>the informational strings to be sent by the <c>FEAT</c> command.</returns>
        IEnumerable<string> BuildInfo(Type reference, IFtpConnectionContext connectionContext);
    }
}
