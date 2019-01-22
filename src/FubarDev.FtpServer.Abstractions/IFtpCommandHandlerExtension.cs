// <copyright file="IFtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

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
        bool? IsLoginRequired { get; }

        /// <summary>
        /// Gets a name of the command this extension is for.
        /// </summary>
        [NotNull]
        string ExtensionFor { get; }

        /// <summary>
        /// Called to initialize the connection data.
        /// </summary>
        void InitializeConnectionData();
    }
}
