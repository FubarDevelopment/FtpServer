// <copyright file="ICommandInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace TestFtpServer.Shell
{
    /// <summary>
    /// Interface for the command information.
    /// </summary>
    public interface ICommandInfo
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the alternative names.
        /// </summary>
        IReadOnlyCollection<string> AlternativeNames { get; }

        /// <summary>
        /// Gets the sub-commands.
        /// </summary>
        IReadOnlyCollection<ICommandInfo> SubCommands { get; }
    }
}
