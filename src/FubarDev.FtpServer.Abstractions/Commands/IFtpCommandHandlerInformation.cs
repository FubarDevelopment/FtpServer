// <copyright file="IFtpCommandHandlerInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Information about an FTP command handler.
    /// </summary>
    public interface IFtpCommandHandlerInformation : IFtpCommandInformation
    {
        /// <summary>
        /// Gets the type of the FTP command handler.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets a value indicating whether this command is extensible.
        /// </summary>
        bool IsExtensible { get; }
    }
}
