// <copyright file="FtpCommandContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// The context in which the command gets executed.
    /// </summary>
    public class FtpCommandContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandContext"/> class.
        /// </summary>
        /// <param name="command">The FTP command.</param>
        public FtpCommandContext([NotNull] FtpCommand command)
        {
            Command = command;
        }

        /// <summary>
        /// Gets the FTP command to be executed.
        /// </summary>
        [NotNull]
        public FtpCommand Command { get; }

        /// <summary>
        /// Gets or sets the FTP connection.
        /// </summary>
        [CanBeNull]
        public IFtpConnection Connection { get; set; }
    }
}
