// <copyright file="FtpExecutionContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// A specialized context for the <see cref="IFtpCommandMiddleware"/>.
    /// </summary>
    public class FtpExecutionContext : FtpContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpExecutionContext"/> class.
        /// </summary>
        /// <param name="ftpContext">The FTP context.</param>
        /// <param name="commandHandler">The FTP command handler.</param>
        /// <param name="commandAborted">The cancellation token signalling an aborted command.</param>
        public FtpExecutionContext(
            FtpContext ftpContext,
            IFtpCommandBase commandHandler,
            CancellationToken commandAborted)
            : base(ftpContext.Command, ftpContext.ServerCommandWriter, ftpContext.Connection)
        {
            CommandHandler = commandHandler;
            CommandAborted = commandAborted;
        }

        /// <summary>
        /// Gets the selected command handler.
        /// </summary>
        public IFtpCommandBase CommandHandler { get; }

        /// <summary>
        /// Gets the cancellation token for an aborted command.
        /// </summary>
        public CancellationToken CommandAborted { get; }
    }
}
