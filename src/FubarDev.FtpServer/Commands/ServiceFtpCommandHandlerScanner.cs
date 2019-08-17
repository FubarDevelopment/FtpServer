// <copyright file="ServiceFtpCommandHandlerScanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Uses DI to get the FTP command handlers.
    /// </summary>
    [Obsolete]
    public class ServiceFtpCommandHandlerScanner : IFtpCommandHandlerScanner
    {
        private readonly IReadOnlyCollection<IFtpCommandHandlerInstanceInformation> _handlerInformation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFtpCommandHandlerScanner"/> class.
        /// </summary>
        /// <param name="commandHandlers">The FTP command handlers.</param>
        /// <param name="logger">The logger.</param>
        public ServiceFtpCommandHandlerScanner(
            IEnumerable<IFtpCommandHandler> commandHandlers,
            ILogger<ServiceFtpCommandHandlerScanner>? logger = null)
        {
            _handlerInformation = commandHandlers.SelectMany(x => x.GetInformation()).ToList();

            // Write warning about obsolete functionality.
            foreach (var information in _handlerInformation)
            {
                var message =
                    $"The command handler of type {information.Instance.GetType()}" +
                    $" for {information.Name} was registered via dependency injection." +
                    " This will not be supported in version 4.0 and is currently obsoleted." +
                    " Please create and register your own implementation of IFtpCommandHandlerScanner or" +
                    " use the AssemblyFtpCommandHandlerScanner.";
                logger?.LogWarning(message);
                Debug.WriteLine($"WARNING: {message}");
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerInformation> Search()
        {
            return _handlerInformation;
        }
    }
}
