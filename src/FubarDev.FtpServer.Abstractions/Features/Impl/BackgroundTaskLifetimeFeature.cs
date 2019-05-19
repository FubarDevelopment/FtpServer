// <copyright file="BackgroundTaskLifetimeFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IBackgroundTaskLifetimeFeature"/>.
    /// </summary>
    public class BackgroundTaskLifetimeFeature : IBackgroundTaskLifetimeFeature
    {
        [NotNull]
        private readonly IBackgroundCommandHandler _backgroundCommandHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskLifetimeFeature"/> class.
        /// </summary>
        /// <param name="command">The FTP command to be run in the background.</param>
        /// <param name="backgroundCommandHandler">The background command handler.</param>
        /// <param name="commandHandler">The FTP command handler.</param>
        public BackgroundTaskLifetimeFeature(
            [NotNull] FtpCommand command,
            [NotNull] IBackgroundCommandHandler backgroundCommandHandler,
            [NotNull] IFtpCommandBase commandHandler)
        {
            _backgroundCommandHandler = backgroundCommandHandler;
            Command = command;
            Handler = commandHandler;
            Task = backgroundCommandHandler.Execute(commandHandler, command);
        }

        /// <inheritdoc />
        public FtpCommand Command { get; }

        /// <inheritdoc />
        public IFtpCommandBase Handler { get; }

        /// <inheritdoc />
        public Task<IFtpResponse> Task { get; }

        /// <inheritdoc />
        public void Abort()
        {
            _backgroundCommandHandler.Cancel();
        }
    }
}
