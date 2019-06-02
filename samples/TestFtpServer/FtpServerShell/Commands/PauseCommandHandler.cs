// <copyright file="PauseCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;

using JetBrains.Annotations;

namespace TestFtpServer.FtpServerShell.Commands
{
    /// <summary>
    /// The <c>PAUSE</c> command.
    /// </summary>
    public class PauseCommandHandler : IRootCommandInfo, IExecutableCommandInfo
    {
        [NotNull]
        private readonly IFtpServer _ftpServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PauseCommandHandler"/> class.
        /// </summary>
        /// <param name="ftpServer">The FTP server.</param>
        public PauseCommandHandler([NotNull] IFtpServer ftpServer)
        {
            _ftpServer = ftpServer;
        }

        /// <inheritdoc />
        public string Name { get; } = "pause";

        /// <inheritdoc />
        public IReadOnlyCollection<string> AlternativeNames { get; } = new string[0];

        /// <inheritdoc />
        public IReadOnlyCollection<ICommandInfo> SubCommands { get; } = new ICommandInfo[0];

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _ftpServer.PauseAsync(cancellationToken);
        }
    }
}
