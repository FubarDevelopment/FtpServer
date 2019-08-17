// <copyright file="ContinueCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JKang.IpcServiceFramework;

namespace TestFtpServer.Shell.Commands
{
    /// <summary>
    /// The <c>CONTINUE</c> command.
    /// </summary>
    public class ContinueCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IpcServiceClient<Api.IFtpServerHost> _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueCommandHandler"/> class.
        /// </summary>
        /// <param name="client">The client to be used to communicate with the FTP server.</param>
        public ContinueCommandHandler(IpcServiceClient<Api.IFtpServerHost> client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public string Name { get; } = "continue";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new[] { "resume" };

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = Array.Empty<ICommandInfo>();

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _client.InvokeAsync(host => host.ContinueAsync(), cancellationToken);
        }
    }
}
