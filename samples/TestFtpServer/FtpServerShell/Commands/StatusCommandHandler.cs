// <copyright file="StatusCommandHandler.cs" company="Fubar Development Junker">
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
    /// The <c>STATUS</c> command.
    /// </summary>
    public class StatusCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IReadOnlyCollection<ISimpleModuleInfo> _moduleInfoItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCommandHandler"/> class.
        /// </summary>
        /// <param name="moduleInfoItems">The registered modules.</param>
        public StatusCommandHandler(
            IEnumerable<IModuleInfo> moduleInfoItems)
        {
            _moduleInfoItems = moduleInfoItems.OfType<ISimpleModuleInfo>().ToList();
        }

        /// <inheritdoc />
        public string Name { get; } = "status";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new string[0];

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = new ICommandInfo[0];

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var items = _moduleInfoItems.SelectMany(x => x.GetInfo()).ToList();
            var maxLabelLength = items.Max(x => x.label.Length);
            var formatString = $"{{0,{-maxLabelLength}}} = {{1}}";
            foreach (var (label, value) in items)
            {
                Console.WriteLine(formatString, label, value);
            }

            return Task.CompletedTask;
        }
    }
}
