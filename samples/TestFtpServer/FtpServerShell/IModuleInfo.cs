// <copyright file="IModuleInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace TestFtpServer.FtpServerShell
{
    /// <summary>
    /// Module information.
    /// </summary>
    public interface IModuleInfo
    {
        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        [NotNull]
        string Name { get; }
    }
}
