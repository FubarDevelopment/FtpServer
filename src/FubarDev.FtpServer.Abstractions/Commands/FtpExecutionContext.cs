// <copyright file="FtpExecutionContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Commands
{
    public class FtpExecutionContext : FtpContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpExecutionContext"/> class.
        /// </summary>
        /// <param name="ftpContext">The FTP context.</param>
        /// <param name="commandHandler">The FTP command handler.</param>
        /// <param name="commandAborted">The cancellation token signalling an aborted command.</param>
        public FtpExecutionContext(
            [NotNull] FtpContext ftpContext,
            [NotNull] IFtpCommandBase commandHandler,
            CancellationToken commandAborted)
            : base(ftpContext.Command, ftpContext.ServerCommandWriter, ftpContext.Connection)
        {
            CommandHandler = commandHandler;
            CommandAborted = commandAborted;
        }

        [NotNull]
        public IFtpCommandBase CommandHandler { get; }

        public CancellationToken CommandAborted { get; }
    }
}
