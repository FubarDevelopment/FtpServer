// <copyright file="HelpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestFtpServer.Shell.Commands
{
    /// <summary>
    /// The <c>HELP</c> command.
    /// </summary>
    public class HelpCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IShellStatus _status;

        public HelpCommandHandler(
            IShellStatus status)
        {
            _status = status;
        }

        /// <inheritdoc />
        public string Name { get; } = "help";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = Array.Empty<string>();

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = Array.Empty<ICommandInfo>();

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("help          - Show help");
            Console.WriteLine("exit          - Close server");
            Console.WriteLine("pause         - Pause accepting clients");
            Console.WriteLine("continue      - Continue accepting clients");
            Console.WriteLine("stop          - Stop the server");
            Console.WriteLine("status        - Show server status");
            Console.WriteLine("show <module> - Show module information");

            if (_status.ExtendedModuleInfoName.Count != 0)
            {
                Console.WriteLine();
                Console.WriteLine("Modules:");
                foreach (var moduleName in _status.ExtendedModuleInfoName)
                {
                    Console.WriteLine("\t{0}", moduleName);
                }
            }

            return Task.CompletedTask;
        }
    }
}
