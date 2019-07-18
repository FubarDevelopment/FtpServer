// <copyright file="IFtpCommandHandlerExtensionInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.FtpServer.Commands;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Interface for an FTP command handler extension.
    /// </summary>
    public interface IFtpCommandHandlerExtensionInformation : IFtpCommandInformation
    {
        /// <summary>
        /// Gets the type of the FTP command handler.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the FTP command handler that this one is an extension of.
        /// </summary>
        IFtpCommandHandlerInformation ExtensionOf { get; }
    }
}
