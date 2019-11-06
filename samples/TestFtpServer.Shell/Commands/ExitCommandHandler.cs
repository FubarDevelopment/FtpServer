// <copyright file="ExitCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JKang.IpcServiceFramework;

using TestFtpServer.Api;

namespace TestFtpServer.Shell.Commands
{
    /// <summary>
    /// The <c>EXIT</c> command.
    /// </summary>
    public class ExitCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IShellStatus _status;
        private readonly IAsyncEnumerable<ICommandInfo> _subCommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="IExecutableCommandInfo"/> class.
        /// </summary>
        /// <param name="client">The IPC client.</param>
        /// <param name="status">The shell status.</param>
        public ExitCommandHandler(
            IpcServiceClient<IFtpServerHost> client,
            IShellStatus status)
        {
            _status = status;
            _subCommands = new ICommandInfo[]
                {
                    new CloseConnectionCommandHandler(client),
                }
               .ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public string Name { get; } = "exit";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new[] { "quit", "close" };

        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        public IAsyncEnumerable<ICommandInfo> GetSubCommandsAsync(CancellationToken cancellationToken)
            => _subCommands;

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _status.Closed = true;
            return Task.CompletedTask;
        }
    }
}
