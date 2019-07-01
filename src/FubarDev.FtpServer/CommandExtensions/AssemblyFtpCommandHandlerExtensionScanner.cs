// <copyright file="AssemblyFtpCommandHandlerExtensionScanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.Commands;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Scans the given assemblies for FTP command handlers.
    /// </summary>
    public class AssemblyFtpCommandHandlerExtensionScanner : IFtpCommandHandlerExtensionScanner
    {
        [NotNull]
        private readonly Dictionary<string, IFtpCommandHandlerInformation> _commandHandlers;

        [NotNull]
        private readonly ILookup<Type, IFtpCommandHandlerInformation> _commandHandlersByType;

        [CanBeNull]
        private readonly ILogger<AssemblyFtpCommandHandlerExtensionScanner> _logger;

        [NotNull]
        [ItemNotNull]
        private readonly Assembly[] _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyFtpCommandHandlerExtensionScanner"/> class.
        /// </summary>
        /// <param name="commandHandlerProvider">The provider for the FTP commands.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="assemblies">The assemblies to scan for FTP command handlers.</param>
        public AssemblyFtpCommandHandlerExtensionScanner(
            [NotNull] IFtpCommandHandlerProvider commandHandlerProvider,
            [CanBeNull] ILogger<AssemblyFtpCommandHandlerExtensionScanner> logger = null,
            [NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            var cmdHandlers = commandHandlerProvider.CommandHandlers.ToList();
            _commandHandlers = cmdHandlers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            _commandHandlersByType = cmdHandlers.ToLookup(x => x.Type);
            _logger = logger;
            _assemblies = assemblies;
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerExtensionInformation> Search()
        {
            return _assemblies.SelectMany(Search);
        }

        private static bool IsCommandHandlerExtensionClass(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
            {
                return false;
            }

            if (typeInfo.IsAbstract)
            {
                return false;
            }

            if (typeInfo.IsGenericTypeDefinition)
            {
                return false;
            }

            if (!typeof(IFtpCommandHandlerExtension).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return false;
            }

            return true;
        }

        private IEnumerable<IFtpCommandHandlerExtensionInformation> Search([NotNull] Assembly assembly)
        {
            foreach (var typeInfo in assembly.DefinedTypes.Where(IsCommandHandlerExtensionClass))
            {
                var attributes = typeInfo.GetCustomAttributes<FtpCommandHandlerExtensionAttribute>().ToList();
                if (attributes.Count == 0)
                {
                    // Missing attribute?
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    if (!_commandHandlers.TryGetValue(attribute.ExtensionOf, out var commandHandlerInformation))
                    {
                        _logger.LogWarning($"No command handler found for ID {attribute.ExtensionOf}.");
                        continue;
                    }

                    var matchingCommandHandlers = _commandHandlersByType[commandHandlerInformation.Type];
                    foreach (var matchingCommandHandler in matchingCommandHandlers)
                    {
                        yield return typeInfo.AsType().GetInformation(matchingCommandHandler, attribute);
                    }
                }
            }
        }
    }
}
