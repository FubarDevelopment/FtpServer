// <copyright file="IFtpCommandHandlerExtensionInstanceInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// Interface to get the instance for a command handler extension created through dependency injection.
    /// </summary>
    [Obsolete]
    public interface IFtpCommandHandlerExtensionInstanceInformation : IFtpCommandHandlerExtensionInformation
    {
        /// <summary>
        /// Gets the FTP command handler extension instance.
        /// </summary>
        IFtpCommandHandlerExtension Instance { get; }
    }
}
