// <copyright file="IExtendedModuleInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace TestFtpServer.ServerInfo
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
        IEnumerable<string> GetExtendedInfo();
    }
}
