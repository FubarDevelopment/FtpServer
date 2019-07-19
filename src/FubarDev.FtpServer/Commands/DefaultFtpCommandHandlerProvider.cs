// <copyright file="DefaultFtpCommandHandlerProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Default implementation of <see cref="IFtpCommandHandlerProvider"/>.
    /// </summary>
    public class DefaultFtpCommandHandlerProvider : IFtpCommandHandlerProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFtpCommandHandlerProvider"/> class.
        /// </summary>
        /// <param name="scanners">The scanners to search for FTP command handlers.</param>
        /// <param name="logger">The logger.</param>
        public DefaultFtpCommandHandlerProvider(
            IEnumerable<IFtpCommandHandlerScanner> scanners,
            ILogger<DefaultFtpCommandHandlerProvider>? logger = null)
        {
            CommandHandlers = GetSanitizedCommandHandlers(scanners, logger).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerInformation> CommandHandlers { get; }

        private IEnumerable<IFtpCommandHandlerInformation> GetSanitizedCommandHandlers(
            IEnumerable<IFtpCommandHandlerScanner> scanners,
            ILogger<DefaultFtpCommandHandlerProvider>? logger = null)
        {
            var commandHandlers = scanners.SelectMany(x => x.Search())
               .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var item in commandHandlers)
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
