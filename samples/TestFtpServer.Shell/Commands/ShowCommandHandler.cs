// <copyright file="ShowCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JKang.IpcServiceFramework;

using TestFtpServer.Api;

namespace TestFtpServer.Shell.Commands
{
    /// <summary>
    /// The <c>SHOW</c> command.
    /// </summary>
    public class ShowCommandHandler : IRootCommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowCommandHandler"/> class.
        /// </summary>
        /// <param name="client">The client to be used to communicate with the FTP server.</param>
        /// <param name="status">The status of the shell.</param>
        public ShowCommandHandler(
            IpcServiceClient<IFtpServerHost> client,
            IShellStatus status)
        {
            SubCommands = status.ExtendedModuleInfoName
               .Select(x => new ModuleCommandInfo(client, x))
               .ToList();
        }

        /// <inheritdoc />
        public string Name { get; } = "show";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; }

        private class ModuleCommandInfo : IExecutableCommandInfo
        {
            private readonly IpcServiceClient<IFtpServerHost> _client;

            /// <summary>
            /// Initializes a new instance of the <see cref="ModuleCommandInfo"/> class.
            /// </summary>
            /// <param name="client">The client to be used to communicate with the FTP server.</param>
            /// <param name="moduleName">The name of the module.</param>
            public ModuleCommandInfo(
                IpcServiceClient<IFtpServerHost> client,
                string moduleName)
            {
                _client = client;
                Name = moduleName;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

            /// <inheritdoc />
            public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = Array.Empty<ICommandInfo>();

            /// <inheritdoc />
            public async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                var info = await _client.InvokeAsync(host => host.GetExtendedModuleInfo(Name), cancellationToken)
                   .ConfigureAwait(false);

                if (!info.TryGetValue(Name, out var lines))
                {
                    return;
                }

                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }
            }
        }
    }
}
