// <copyright file="StopCommandHandler.cs" company="Fubar Development Junker">
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
    /// The <c>STOP</c> command.
    /// </summary>
    public class StopCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IpcServiceClient<Api.IFtpServerHost> _client;
        private readonly IShellStatus _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="StopCommandHandler"/> class.
        /// </summary>
        /// <param name="client">The client to be used to communicate with the FTP server.</param>
        /// <param name="status">The shell status.</param>
        public StopCommandHandler(
            IpcServiceClient<Api.IFtpServerHost> client,
            IShellStatus status)
        {
            _client = client;
            _status = status;
        }

        /// <inheritdoc />
        public string Name { get; } = "stop";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = Array.Empty<ICommandInfo>();

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _status.Closed = true;
            return _client.InvokeAsync(host => host.StopAsync(), cancellationToken);
        }
    }
}
