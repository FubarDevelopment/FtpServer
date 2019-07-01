// <copyright file="ServiceFtpCommandHandlerScanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

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
        public ServiceFtpCommandHandlerScanner(IEnumerable<IFtpCommandHandler> commandHandlers)
        {
            _handlerInformation = commandHandlers.SelectMany(x => x.GetInformation()).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerInformation> Search()
        {
            return _handlerInformation;
        }
    }
}
