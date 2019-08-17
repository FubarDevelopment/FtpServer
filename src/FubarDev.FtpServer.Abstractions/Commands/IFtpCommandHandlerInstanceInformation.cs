// <copyright file="IFtpCommandHandlerInstanceInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Interface to get the instance for a command handler created through dependency injection.
    /// </summary>
    [Obsolete]
    public interface IFtpCommandHandlerInstanceInformation : IFtpCommandHandlerInformation
    {
        /// <summary>
        /// Gets the FTP command handler instance.
        /// </summary>
        IFtpCommandHandler Instance { get; }
    }
}
