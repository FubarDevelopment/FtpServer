// <copyright file="IFtpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface that an FTP command handler has to implement.
    /// </summary>
    public interface IFtpCommandHandler : IFtpCommandBase
    {
        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command.
        /// </summary>
        bool IsLoginRequired { get; }

        /// <summary>
        /// Gets a value indicating whether this command is abortable.
        /// </summary>
        bool IsAbortable { get; }

        /// <summary>
        /// Gets a collection of features supported by this command handler.
        /// </summary>
        /// <returns>A list of features supported by this command handler.</returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IFeatureInfo> GetSupportedFeatures();

        /// <summary>
        /// Gets a collection of command handler extensions provided by this command handler.
        /// </summary>
        /// <returns>A collection of command handler extensions provided by this command handler.</returns>
        [NotNull]
        [ItemNotNull]
        [Obsolete("All IFtpCommandHandlerExtension implementations are now stand-alone.")]
        IEnumerable<IFtpCommandHandlerExtension> GetExtensions();
    }
}
