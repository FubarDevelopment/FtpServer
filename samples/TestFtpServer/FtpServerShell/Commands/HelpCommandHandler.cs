// <copyright file="HelpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace TestFtpServer.FtpServerShell.Commands
{
    /// <summary>
    /// The <c>HELP</c> command.
    /// </summary>
    public class HelpCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        [NotNull]
        [ItemNotNull]
        private readonly IReadOnlyCollection<IModuleInfo> _moduleInfoItems;

        public HelpCommandHandler([NotNull, ItemNotNull] IEnumerable<IModuleInfo> moduleInfoItems)
        {
            _moduleInfoItems = moduleInfoItems.ToList();
        }

        /// <inheritdoc />
        public string Name { get; } = "help";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new string[0];

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = new ICommandInfo[0];

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("help          - Show help");
            Console.WriteLine("exit          - Close server");
            Console.WriteLine("pause         - Pause accepting clients");
            Console.WriteLine("continue      - Continue accepting clients");
            Console.WriteLine("status        - Show server status");
            Console.WriteLine("show <module> - Show module information");

            var extendedModules = _moduleInfoItems.OfType<IExtendedModuleInfo>().ToList();
            if (extendedModules.Count != 0)
            {
                Console.WriteLine();
                Console.WriteLine("Modules:");
                foreach (var moduleName in extendedModules.Select(x => x.Name).Distinct())
                {
                    Console.WriteLine("\t{0}", moduleName);
                }
            }

            return Task.CompletedTask;
        }
    }
}
