// <copyright file="ISimpleModuleInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace TestFtpServer.ServerInfo
{
    /// <summary>
    /// Simple module information.
    /// </summary>
    public interface ISimpleModuleInfo : IModuleInfo
    {
        /// <summary>
        /// Gets labels and values to be printed.
        /// </summary>
        /// <returns></returns>
        IEnumerable<(string label, string value)> GetInfo();
    }
}
