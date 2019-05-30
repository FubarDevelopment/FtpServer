// <copyright file="ContinueCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;

namespace TestFtpServer.FtpServerShell.Commands
{
    /// <summary>
    /// The <c>CONTINUE</c> command.
    /// </summary>
    public class ContinueCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        private readonly IFtpServer _ftpServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueCommandHandler"/> class.
        /// </summary>
        /// <param name="ftpServer">The FTP server.</param>
        public ContinueCommandHandler(IFtpServer ftpServer)
        {
            _ftpServer = ftpServer;
        }

        /// <inheritdoc />
        public string Name { get; } = "continue";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new[] { "resume" };

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = new ICommandInfo[0];

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _ftpServer.ContinueAsync(cancellationToken);
        }
    }
}
