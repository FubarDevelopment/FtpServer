// <copyright file="DefaultFtpCommandHandlerProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

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
        public DefaultFtpCommandHandlerProvider(IEnumerable<IFtpCommandHandlerScanner> scanners)
        {
            CommandHandlers = scanners.SelectMany(x => x.Search()).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerInformation> CommandHandlers { get; }
    }
}
