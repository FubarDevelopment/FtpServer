// <copyright file="BackgroundTaskLifetimeFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.Features.Impl
{
    public class BackgroundTaskLifetimeFeature : IBackgroundTaskLifetimeFeature
    {
        [NotNull]
        private readonly IBackgroundCommandHandler _backgroundCommandHandler;

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

        public void Abort()
        {
            _backgroundCommandHandler.Cancel();
        }
    }
}
