// <copyright file="IExtendedModuleInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace TestFtpServer.FtpServerShell
{
    /// <summary>
    /// Extended module information.
    /// </summary>
    public interface IExtendedModuleInfo : IModuleInfo
    {
        /// <summary>
        /// Gets the extended information.
        /// </summary>
        /// <returns>The lines to be printed.</returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<string> GetExtendedInfo();
    }
}
