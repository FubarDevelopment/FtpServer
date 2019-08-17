// <copyright file="IFtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Interface for command handler extensions.
    /// </summary>
    public interface IFtpCommandHandlerExtension : IFtpCommandBase
    {
        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command.
        /// </summary>
        [Obsolete("Use the FtpCommandHandlerExtensionAttribute together with an additional IFtpCommandHandlerExtensionScanner.")]
        bool? IsLoginRequired { get; }

        /// <summary>
        /// Gets a name of the command this extension is for.
        /// </summary>
        [Obsolete("Use the FtpCommandHandlerExtensionAttribute together with an additional IFtpCommandHandlerExtensionScanner.")]
        string ExtensionFor { get; }

        /// <summary>
        /// Called to initialize the connection data.
        /// </summary>
        void InitializeConnectionData();
    }
}
