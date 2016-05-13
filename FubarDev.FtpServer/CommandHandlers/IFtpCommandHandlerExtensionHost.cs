// <copyright file="IFtpCommandHandlerExtensionHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.FtpServer.CommandExtensions;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Interface indicating that a command handler can act as an extension host.
    /// </summary>
    public interface IFtpCommandHandlerExtensionHost
    {
        /// <summary>
        /// Gets the extensions hosted by the <see cref="FtpCommandHandler"/>
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IDictionary<string, FtpCommandHandlerExtension> Extensions { get; }
    }
}
