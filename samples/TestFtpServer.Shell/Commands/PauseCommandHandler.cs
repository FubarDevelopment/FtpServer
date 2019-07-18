// <copyright file="PauseCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JKang.IpcServiceFramework;

using TestFtpServer.Api;

namespace TestFtpServer.Shell.Commands
{
    /// <summary>
    /// The <c>PAUSE</c> command.
    /// </summary>
    public class PauseCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IpcServiceClient<IFtpServerHost> _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="PauseCommandHandler"/> class.
        /// </summary>
        /// <param name="client">The client to be used to communicate with the FTP server.</param>
        public PauseCommandHandler(IpcServiceClient<IFtpServerHost> client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public string Name { get; } = "pause";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = Array.Empty<ICommandInfo>();

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _client.InvokeAsync(host => host.PauseAsync(), cancellationToken);
        }
    }
}
