// <copyright file="CloseConnectionCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using JKang.IpcServiceFramework;

using TestFtpServer.Api;

namespace TestFtpServer.Shell.Commands
{
    /// <summary>
    /// Command handler for closing a client FTP connection.
    /// </summary>
    public class CloseConnectionCommandHandler : ICommandInfo
    {
        private readonly IpcServiceClient<IFtpServerHost> _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseConnectionCommandHandler"/> class.
        /// </summary>
        /// <param name="client">The IPC client.</param>
        public CloseConnectionCommandHandler(IpcServiceClient<IFtpServerHost> client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public string Name { get; } = "connection";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public async IAsyncEnumerable<ICommandInfo> GetSubCommandsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var connections = await _client
               .InvokeAsync(host => host.GetConnections(), cancellationToken)
               .ConfigureAwait(false);
            foreach (var connection in connections)
            {
                yield return new CloseConnectionFinalCommandHandler(_client, connection.Id);
            }
        }

        private class CloseConnectionFinalCommandHandler : IExecutableCommandInfo
        {
            private readonly IpcServiceClient<IFtpServerHost> _client;

            /// <summary>
            /// Initializes a new instance of the <see cref="CloseConnectionFinalCommandHandler"/> class.
            /// </summary>
            /// <param name="client">The IPC client.</param>
            /// <param name="connectionId">The FTP connection ID.</param>
            public CloseConnectionFinalCommandHandler(
                IpcServiceClient<IFtpServerHost> client,
                string connectionId)
            {
                _client = client;
                Name = connectionId;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

            /// <inheritdoc />
            public IAsyncEnumerable<ICommandInfo> GetSubCommandsAsync(CancellationToken cancellationToken)
                => AsyncEnumerable.Empty<ICommandInfo>();

            /// <inheritdoc />
            public Task ExecuteAsync(CancellationToken cancellationToken)
            {
                return _client.InvokeAsync(host => host.CloseConnectionAsync(Name), cancellationToken);
            }
        }
    }
}
