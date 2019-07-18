// <copyright file="DefaultFtpCommandHandlerExtensionProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

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
        public DefaultFtpCommandHandlerExtensionProvider(IEnumerable<IFtpCommandHandlerExtensionScanner> scanners)
        {
            CommandHandlerExtensions = scanners.SelectMany(x => x.Search()).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerExtensionInformation> CommandHandlerExtensions { get; }
    }
}
