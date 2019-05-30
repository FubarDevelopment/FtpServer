// <copyright file="ShowCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestFtpServer.FtpServerShell.Commands
{
    /// <summary>
    /// The <c>SHOW</c> command.
    /// </summary>
    public class ShowCommandHandler : IRootCommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowCommandHandler"/> class.
        /// </summary>
        /// <param name="moduleInfoItems">The registered modules.</param>
        public ShowCommandHandler(IEnumerable<IModuleInfo> moduleInfoItems)
        {
            SubCommands = moduleInfoItems
               .OfType<IExtendedModuleInfo>()
               .Select(x => new ModuleCommandInfo(x))
               .ToList();
        }

        /// <inheritdoc />
        public string Name { get; } = "show";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new string[0];

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; }

        private class ModuleCommandInfo : IExecutableCommandInfo
        {
            private readonly IExtendedModuleInfo _extendedModuleInfo;

            public ModuleCommandInfo(IExtendedModuleInfo extendedModuleInfo)
            {
                _extendedModuleInfo = extendedModuleInfo;
                Name = extendedModuleInfo.Name;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<string> AlternativeNames { get; } = new string[0];

            /// <inheritdoc />
            public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = new ICommandInfo[0];

            /// <inheritdoc />
            public Task ExecuteAsync(CancellationToken cancellationToken)
            {
                foreach (var line in _extendedModuleInfo.GetExtendedInfo())
                {
                    Console.WriteLine(line);
                }

                return Task.CompletedTask;
            }
        }
    }
}
