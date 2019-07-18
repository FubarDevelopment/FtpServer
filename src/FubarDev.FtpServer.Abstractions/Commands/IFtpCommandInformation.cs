// <copyright file="IFtpCommandInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Generic information about an FTP command.
    /// </summary>
    public interface IFtpCommandInformation
    {
        /// <summary>
        /// Gets the name of the FTP command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether a login is required to execute this command.
        /// </summary>
        bool IsLoginRequired { get; }

        /// <summary>
        /// Gets a value indicating whether this command is abortable.
        /// </summary>
        bool IsAbortable { get; }
    }
}
