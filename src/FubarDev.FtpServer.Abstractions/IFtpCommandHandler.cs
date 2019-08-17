// <copyright file="IFtpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface that an FTP command handler has to implement.
    /// </summary>
#pragma warning disable 618
    public interface IFtpCommandHandler : IFtpCommandBase, IFeatureHost
#pragma warning restore 618
    {
        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command.
        /// </summary>
        [Obsolete("Information about an FTP command handler can be queried through the IFtpCommandHandlerProvider service.")]
        bool IsLoginRequired { get; }

        /// <summary>
        /// Gets a value indicating whether this command is abortable.
        /// </summary>
        [Obsolete("Information about an FTP command handler can be queried through the IFtpCommandHandlerProvider service.")]
        bool IsAbortable { get; }

        /// <summary>
        /// Gets a collection of command handler extensions provided by this command handler.
        /// </summary>
        /// <returns>A collection of command handler extensions provided by this command handler.</returns>
        [Obsolete("All IFtpCommandHandlerExtension implementations are now stand-alone.", true)]
        IEnumerable<IFtpCommandHandlerExtension> GetExtensions();
    }
}
