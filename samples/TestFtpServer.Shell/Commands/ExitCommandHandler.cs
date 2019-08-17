// <copyright file="ExitCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestFtpServer.Shell.Commands
{
    /// <summary>
    /// The <c>EXIT</c> command.
    /// </summary>
    public class ExitCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IShellStatus _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="IExecutableCommandInfo"/> class.
        /// </summary>
        /// <param name="status">The shell status.</param>
        public ExitCommandHandler(IShellStatus status)
        {
            _status = status;
        }

        /// <inheritdoc />
        public string Name { get; } = "exit";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new[] { "quit" };

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = Array.Empty<ICommandInfo>();

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _status.Closed = true;
            return Task.CompletedTask;
        }
    }
}
