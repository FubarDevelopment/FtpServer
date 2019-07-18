// <copyright file="IShellStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace TestFtpServer.Shell
{
    /// <summary>
    /// Status of the FTP server shell.
    /// </summary>
    public interface IShellStatus
    {
        /// <summary>
        /// Gets or sets the simple module information names.
        /// </summary>
        ICollection<string> SimpleModuleInfoNames { get; }

        /// <summary>
        /// Gets or sets the extended module information names.
        /// </summary>
        ICollection<string> ExtendedModuleInfoName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the FTP server should be closed.
        /// </summary>
        bool Closed { get; set; }
    }
}
