// <copyright file="FtpCommandSelection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Information about the selected FTP command handler.
    /// </summary>
    public class FtpCommandSelection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandSelection"/> class.
        /// </summary>
        /// <param name="handler">The FTP command handler.</param>
        /// <param name="commandContext">The FTP command context.</param>
        /// <param name="isLoginRequired">Indicates whether a successful login is required.</param>
        public FtpCommandSelection(IFtpCommandBase handler, FtpCommandContext commandContext, bool isLoginRequired)
        {
            Handler = handler;
            CommandContext = commandContext;
            IsLoginRequired = isLoginRequired;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public FtpCommandContext CommandContext { get; }

        /// <summary>
        /// Gets the command handler.
        /// </summary>
        public IFtpCommandBase Handler { get; }

        /// <summary>
        /// Gets a value indicating whether a successful login is required.
        /// </summary>
        public bool IsLoginRequired { get; }
    }
}
