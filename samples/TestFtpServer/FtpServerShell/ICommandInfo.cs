// <copyright file="ICommandInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace TestFtpServer.FtpServerShell
{
    /// <summary>
    /// Interface for the command information.
    /// </summary>
    public interface ICommandInfo
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets the alternative names.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> AlternativeNames { get; }

        /// <summary>
        /// Gets the sub-commands.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<ICommandInfo> SubCommands { get; }
    }
}
