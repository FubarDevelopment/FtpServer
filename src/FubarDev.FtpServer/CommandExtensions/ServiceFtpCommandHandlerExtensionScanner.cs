// <copyright file="ServiceFtpCommandHandlerExtensionScanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FubarDev.FtpServer.Commands;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Uses DI to get the FTP command handlers.
    /// </summary>
    [Obsolete]
    public class ServiceFtpCommandHandlerExtensionScanner : IFtpCommandHandlerExtensionScanner
    {
        private readonly IReadOnlyCollection<IFtpCommandHandlerExtensionInstanceInformation> _extensionInformation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFtpCommandHandlerExtensionScanner"/> class.
        /// </summary>
        /// <param name="commandHandlerProvider">The FTP command handler provider.</param>
        /// <param name="commandHandlerExtensions">The FTP command handler extensions.</param>
        /// <param name="logger">The logger.</param>
        public ServiceFtpCommandHandlerExtensionScanner(
            IFtpCommandHandlerProvider commandHandlerProvider,
            IEnumerable<IFtpCommandHandlerExtension> commandHandlerExtensions,
            ILogger<ServiceFtpCommandHandlerScanner>? logger = null)
        {
            _extensionInformation = CreateInformation(commandHandlerProvider, commandHandlerExtensions, logger).ToList();

            // Write warning about obsolete functionality.
            foreach (var information in _extensionInformation)
            {
                var message =
                    $"The command handler extension of type {information.Instance.GetType()}" +
                    $" for {information.Name} was registered via dependency injection." +
                    " This will not be supported in version 4.0 and is currently obsoleted." +
                    " Please create and register your own implementation of IFtpCommandHandlerExtensionScanner or" +
                    " use the AssemblyFtpCommandHandlerExtensionScanner.";
                logger?.LogWarning(message);
                Debug.WriteLine($"WARNING: {message}");
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerExtensionInformation> Search()
        {
            return _extensionInformation;
        }

        private static IEnumerable<IFtpCommandHandlerExtensionInstanceInformation> CreateInformation(
            IFtpCommandHandlerProvider commandHandlerProvider,
            IEnumerable<IFtpCommandHandlerExtension> commandHandlerExtensions,
            ILogger? logger = null)
        {
            var cmdHandlers = commandHandlerProvider.CommandHandlers.ToList();
            var commandHandlers = cmdHandlers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var commandHandlersByType = cmdHandlers.ToLookup(x => x.Type);

            foreach (var handlerExtension in commandHandlerExtensions)
            {
                if (!commandHandlers.TryGetValue(handlerExtension.ExtensionFor, out var commandHandlerInformation))
                {
                    logger?.LogWarning("No command handler found for ID {commandId}.", handlerExtension.ExtensionFor);
                    continue;
                }

                var matchingCommandHandlers = commandHandlersByType[commandHandlerInformation.Type];
                foreach (var matchingCommandHandler in matchingCommandHandlers)
                {
                    foreach (var extensionInformation in handlerExtension.GetInformation(matchingCommandHandler))
                    {
                        yield return extensionInformation;
                    }
                }
            }
        }
    }
}
