// <copyright file="IFtpCommandActivator.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Activator for an FTP command.
    /// </summary>
    public interface IFtpCommandActivator
    {
        /// <summary>
        /// Gets information about the FTP command to be executed.
        /// </summary>
        /// <param name="context">The FTP command execution context.</param>
        /// <returns>Information about the FTP command to be executed.</returns>
        FtpCommandSelection? Create(FtpCommandHandlerContext context);
    }
}
