// <copyright file="CloseCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using JKang.IpcServiceFramework;

using TestFtpServer.Api;

namespace TestFtpServer.Shell.Commands
{
    public class CloseCommandHandler : IRootCommandInfo
    {
        private readonly IAsyncEnumerable<ICommandInfo> _subCommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseCommandHandler"/> class.
        /// </summary>
        /// <param name="client">The IPC client.</param>
        public CloseCommandHandler(
            IpcServiceClient<IFtpServerHost> client)
        {
            _subCommands = new ICommandInfo[]
                {
                    new CloseConnectionCommandHandler(client),
                }
               .ToAsyncEnumerable();
        }

        /// <inheritdoc />
        public string Name { get; } = "close";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public IAsyncEnumerable<ICommandInfo> GetSubCommandsAsync(CancellationToken cancellationToken)
            => _subCommands;
    }
}
