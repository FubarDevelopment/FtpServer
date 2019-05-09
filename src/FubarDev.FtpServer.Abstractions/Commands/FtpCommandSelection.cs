// <copyright file="FtpCommandSelection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
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
        /// <param name="handlerInformation">The FTP command handler information.</param>
        public FtpCommandSelection(
            [NotNull] IFtpCommandBase handler,
            [NotNull] FtpCommandContext commandContext,
            [NotNull] IFtpCommandInformation handlerInformation)
        {
            Handler = handler;
            CommandContext = commandContext;
            Information = handlerInformation;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        [NotNull]
        public FtpCommandContext CommandContext { get; }

        /// <summary>
        /// Gets the command handler.
        /// </summary>
        [NotNull]
        public IFtpCommandBase Handler { get; }

        /// <summary>
        /// Gets a value indicating whether a successful login is required.
        /// </summary>
        [NotNull]
        public IFtpCommandInformation Information { get; }
    }
}
