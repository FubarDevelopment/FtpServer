// <copyright file="ShellStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace TestFtpServer.Shell
{
    /// <summary>
    /// Status for the FTP shell.
    /// </summary>
    public class ShellStatus : IShellStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellStatus"/> class.
        /// </summary>
        /// <param name="simpleModuleInfoNames">The names of the simple information modules.</param>
        /// <param name="extendedModuleInfoName">The names of the extended information modules.</param>
        public ShellStatus(
            ICollection<string> simpleModuleInfoNames,
            ICollection<string> extendedModuleInfoName)
        {
            SimpleModuleInfoNames = simpleModuleInfoNames;
            ExtendedModuleInfoName = extendedModuleInfoName;
        }

        /// <inheritdoc />
        public ICollection<string> SimpleModuleInfoNames { get; }

        /// <inheritdoc />
        public ICollection<string> ExtendedModuleInfoName { get; }

        /// <inheritdoc />
        public bool Closed { get; set; }
    }
}
