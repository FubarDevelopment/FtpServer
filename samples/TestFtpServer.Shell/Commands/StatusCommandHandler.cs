// <copyright file="StatusCommandHandler.cs" company="Fubar Development Junker">
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
    /// The <c>STATUS</c> command.
    /// </summary>
    public class StatusCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IpcServiceClient<IFtpServerHost> _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCommandHandler"/> class.
        /// </summary>
        /// <param name="client">The client to be used to communicate with the FTP server.</param>
        public StatusCommandHandler(
            IpcServiceClient<IFtpServerHost> client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public string Name { get; } = "status";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = Array.Empty<ICommandInfo>();

        /// <inheritdoc />
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var simpleModules = await _client.InvokeAsync(host => host.GetSimpleModules(), cancellationToken)
               .ConfigureAwait(false);
            if (simpleModules.Count == 0)
            {
                return;
            }

            var simpleModuleItems = await _client.InvokeAsync(
                    host => host.GetSimpleModuleInfo(simpleModules.ToArray()),
                    cancellationToken)
               .ConfigureAwait(false);

            if (simpleModuleItems.Count == 0)
            {
                return;
            }

            var maxLabelLength = simpleModuleItems.SelectMany(x => x.Value).Max(x => x.Key.Length);
            var formatString = $"\t{{0,{-maxLabelLength}}} = {{1}}";
            foreach (var moduleName in simpleModules)
            {
                if (simpleModuleItems.TryGetValue(moduleName, out var items))
                {
                    Console.WriteLine($"{moduleName}:");
                    foreach (var (label, value) in items)
                    {
                        Console.WriteLine(formatString, label, value);
                    }
                }
            }
        }
    }
}
