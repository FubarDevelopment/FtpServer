// <copyright file="ICommandInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;

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
        /// <param name="cancellationToken">The cancellation token.</param>
        IAsyncEnumerable<ICommandInfo> GetSubCommandsAsync(CancellationToken cancellationToken);
    }
}
