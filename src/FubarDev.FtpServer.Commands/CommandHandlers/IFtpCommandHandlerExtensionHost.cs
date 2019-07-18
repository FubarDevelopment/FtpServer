// <copyright file="IFtpCommandHandlerExtensionHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Interface indicating that a command handler can act as an extension host.
    /// </summary>
    public interface IFtpCommandHandlerExtensionHost : IFtpCommandHandler
    {
        /// <summary>
        /// Gets or sets the extensions hosted by the <see cref="FtpCommandHandler"/>.
        /// </summary>
        IDictionary<string, IFtpCommandHandlerExtension> Extensions { get; set; }
    }
}
