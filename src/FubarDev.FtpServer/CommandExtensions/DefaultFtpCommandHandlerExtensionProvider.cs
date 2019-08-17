// <copyright file="DefaultFtpCommandHandlerExtensionProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Default implementation of <see cref="IFtpCommandHandlerExtensionProvider"/>.
    /// </summary>
    public class DefaultFtpCommandHandlerExtensionProvider : IFtpCommandHandlerExtensionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFtpCommandHandlerExtensionProvider"/> class.
        /// </summary>
        /// <param name="scanners">The scanners to search for FTP command handlers.</param>
        /// <param name="logger">The logger.</param>
        public DefaultFtpCommandHandlerExtensionProvider(
            IEnumerable<IFtpCommandHandlerExtensionScanner> scanners,
            ILogger<DefaultFtpCommandHandlerExtensionProvider>? logger = null)
        {
            CommandHandlerExtensions = GetSanitizedCommandHandlers(scanners, logger).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerExtensionInformation> CommandHandlerExtensions { get; }

        private IEnumerable<IFtpCommandHandlerExtensionInformation> GetSanitizedCommandHandlers(
            IEnumerable<IFtpCommandHandlerExtensionScanner> scanners,
            ILogger<DefaultFtpCommandHandlerExtensionProvider>? logger = null)
        {
            var commandExtensionHandlers = scanners.SelectMany(x => x.Search())
               .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var item in commandExtensionHandlers)
            {
                var handlersPerName = item.ToList();
                if (handlersPerName.Count != 1)
                {
                    foreach (var information in handlersPerName.Take(handlersPerName.Count - 1))
                    {
                        logger?.LogWarning("Duplicate handler for FTP command {name} found, implemented by {type}.", item.Key, information.Type);
                    }
                }

                yield return handlersPerName[handlersPerName.Count - 1];
            }
        }
    }
}
